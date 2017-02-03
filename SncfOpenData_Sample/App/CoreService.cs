using Microsoft.SqlServer.Types;
using NavitiaSharp;
using SncfOpenData.IGN;
using SncfOpenData.IGN.Model;
using SncfOpenData.Model;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SncfOpenData
{
    public class CoreService
    {
        private readonly IGNRepository _ignRepo;
        private readonly SncfRepository _sncfRepo;

        public CoreService(SncfRepository repo, IGNRepository railroads)
        {
            _sncfRepo = repo;
            _ignRepo = railroads;
        }

        /// <summary>
        /// Find nearest IGN nodes for each Sncf stop area.
        /// Done recursively in two passes. One for close matches within 100 m
        /// Second pass with 30km and name checking (breaks in debug for name mismatches)
        /// </summary>
        /// <param name="stopAreasIgn">Pass</param>
        /// <returns></returns>
        public List<StopAreaIGN> MatchStopAreasWithIGNNodes()
        {
            // Work in L93 proj : use of more funcs on geometry types, and distance calculations done in meters

            // Full db read
            #region Full DB read

            Dictionary<int, Troncon> allTroncons = _ignRepo.GetAllTroncons_Lambert93();
            Dictionary<int, Noeud> allNoeuds = _ignRepo.GetAllNoeuds_Lambert93()
                                                            .Where(n => n.Value.Nature == "Gare de voyageurs" || n.Value.Nature == "Gare de voyageurs et de fret")
                                                            .ToDictionary(k => k.Key, k => k.Value);

            #endregion


            #region 1st pass : match within 100 meters (CPU intensive)

            List<StopAreaIGN> stopAreasIgn = new List<StopAreaIGN>();
            Parallel.ForEach(_sncfRepo.StopAreas, area =>
            {
                StopAreaIGN areaIgn = new StopAreaIGN { StopAreaId = area.Id };
                var area2154 = FromCoordToGeometry2154(area.Coord);
                var closestIgnPoints = from noeud in allNoeuds
                                       let dist = noeud.Value.Geometry.STDistance(area2154).Value
                                       where dist < 100
                                       orderby dist
                                       select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = noeud.Value };

                var res = closestIgnPoints.FirstOrDefault();
                if (res != null)
                {

                    Trace.TraceInformation($"{area.Name}: point {res.IGNObject.Toponyme} at {(int)Math.Round(res.Distance, 0)}");
                    areaIgn.IdNoeud = res.IGNObject.Id;
                    areaIgn.NomNoeud = res.IGNObject.Toponyme;
                    areaIgn.DistanceNoeud = res.Distance;

                }
                else
                {
                    Trace.TraceInformation($"{area.Name}: not point found.");
                }
                stopAreasIgn.Add(areaIgn);

            }
            );

            // 2nd pass
            return MatchStopAreasWithIGNNodes_2ndPass(stopAreasIgn, allTroncons, allNoeuds);

            #endregion

        }

        private List<StopAreaIGN> MatchStopAreasWithIGNNodes_2ndPass(List<StopAreaIGN> stopAreasIgn, Dictionary<int, Troncon> allTroncons, Dictionary<int, Noeud> allNoeuds)
        {
            // Work in L93 proj : use of more funcs on geometry types, and distance calculations done in meters

            if (stopAreasIgn != null)
            {

                #region 2nd pass : for non matched, match within 30000 meters (CPU intensive)

                var stopAreasIgn_NonMatched = stopAreasIgn.Where(s => s.HasIGNMatch == false);
                Parallel.ForEach(stopAreasIgn_NonMatched, stopAreaIgn =>
                //foreach (var stopAreaIgn in stopAreasIgn_NonMatched)
                {
                    StopArea area = _sncfRepo.StopAreas.Where(s => s.Id == stopAreaIgn.StopAreaId).Single();

                    var area2154 = FromCoordToGeometry2154(area.Coord);

                    var closestIgnPoints = from noeud in allNoeuds
                                           let dist = noeud.Value.Geometry.STDistance(area2154).Value
                                           where dist < 30000
                                           orderby dist
                                           select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = noeud.Value };

                    if (closestIgnPoints.Any())
                    {
                        var res = closestIgnPoints.First();
                        if (allNoeuds[res.IGNObject.Id].Toponyme.ToUpper().Trim() == area.Name.ToUpper().Trim())
                        {
                            //Trace.TraceInformation($"{area.Name}: point {DBgeomNoeuds2154NAme[res.IGNObject.Key]} at {(int)Math.Round(res.Distance, 0)}");
                            stopAreaIgn.IdNoeud = res.IGNObject.Id;
                            stopAreaIgn.NomNoeud = res.IGNObject.Toponyme;
                            stopAreaIgn.DistanceNoeud = res.Distance;
                        }
                        else
                        {
                            Debug.Fail($"WARN : {area.Name} name does not match with point {res.IGNObject.Toponyme} name. Distance : {(int)Math.Round(res.Distance, 0)}", "");

                        }
                    }
                    else
                    {
                        // Trace.TraceInformation($"{area.Name}: not point found.");
                    }

                }
                );

                #endregion
            }

            return stopAreasIgn;
        }

        /// <summary>
        /// 
        /// </summary>
        public void MatchRoutesWithTronconsIGN()
        {
            //foreach(Line line in _sncfRepo.Lines)
            //{
            //    HashSet<string> linkedRoutesId = new HashSet<string>(line.Routes.Select(lr => lr.Id));
            //    List<StopArea> stopareas = null;
            //    List<Route> routes = _sncfRepo.Routes.Where(r => linkedRoutesId.Contains(r.Id)).ToList();
            //    if (_sncfRepo.LinesStopAreas.TryGetValue(line.Id, out stopareas))
            //    {
            //        List<int> ignNodes = stopareas.Select(sa => _sncfRepo.IgnNodeByStopArea[sa.Id]).ToList();
            //        // TODO : Dijkstra for all troncons within envelope of all line stop areas
            //    }
            //}

            var nodes = _ignRepo.GetAllNoeuds_Lambert93();
            var troncons = _ignRepo.GetAllTroncons_Lambert93();

            foreach (Route route in _sncfRepo.Routes)
            {
                List<StopArea> stopareas = null;
                if (_sncfRepo.RoutesStopAreas.TryGetValue(route.Id, out stopareas))
                {
                    HashSet<int> ignNodes = new HashSet<int>(stopareas.Select(sa => _sncfRepo.IgnNodeByStopArea[sa.Id]));
                    var nodesIds = String.Join<int>(",", ignNodes);

                    SqlGeometry geom = SqlTypesExtensions.PointEmpty_SqlGeometry(2154);

                    foreach (var pointGeom in nodes.Where(kvp => ignNodes.Contains(kvp.Key)).Select(kvp => kvp.Value))
                    {
                        geom = geom.STUnion(pointGeom.Geometry);
                    }
                    geom = geom.STEnvelope();
                    var tronconsInRoute = troncons.Where(kvp => kvp.Value.Geometry.STIntersects(geom).Value == true).ToList();

                    // TODO : Dijkstra for all troncons within envelope of all line stop areas
                }
            }
        }

        public void ShowStopAreasOnMap(SncfRepository _sncfRepo, IGNRepository _ignRepo, string wkt = null)
        {

            SqlGeography polyQuery = wkt == null ? null : SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);
            Dictionary<int, Noeud> noeuds = _ignRepo.GetAllNoeuds_LatLon(polyQuery);


            Dictionary<string, SqlGeography> geogListStopAreas = new Dictionary<string, SqlGeography>();
            IEnumerable<StopArea> stopAreas = _sncfRepo.StopAreas;
            if (polyQuery != null)
            {
                stopAreas = stopAreas.Where(s => FromCoordToGeography(s.Coord).STIntersects(polyQuery).IsTrue);
            }
            foreach (var sp in stopAreas)
            {
                geogListStopAreas.Add(sp.Name + " " + sp.Id, FromCoordToGeography(sp.Coord));
            }
            Dictionary<string, SqlGeography> geogListStopPoints = new Dictionary<string, SqlGeography>();
            IEnumerable<StopPoint> stopPoints = _sncfRepo.StopPoints;
            if (polyQuery != null)
            {
                stopPoints = stopPoints.Where(s => FromCoordToGeography(s.Coord).STIntersects(polyQuery).IsTrue);
            }
            foreach (var sp in stopPoints)
            {
                geogListStopPoints.Add(sp.Name + " " + sp.Id, FromCoordToGeography(sp.Coord));
            }


            SpatialTrace.Enable();
            int i = 0;
            foreach (var g in noeuds)
            {
                if (i % 2 == 0)
                {
                    SpatialTrace.SetLineColor(Colors.Blue);
                }
                else
                {
                    SpatialTrace.SetLineColor(Colors.Red);
                }
                SpatialTrace.TraceGeometry(g.Value.Geometry, $"{g.Value.Id}: {g.Value.Toponyme}", $"{g.Value.Id}: {g.Value.Toponyme}");
                i++;
            }


            //SpatialTrace.SetLineColor(Colors.Red);
            //SpatialTrace.TraceGeometry(geogList120, "Lignes 120", "Lignes 120");


            SpatialTrace.Indent("Stop areas");
            SpatialTrace.SetFillColor(Colors.Green);
            foreach (var kvp in geogListStopAreas)
            {
                SpatialTrace.TraceGeometry(kvp.Value, kvp.Key, kvp.Key);
            }

            SpatialTrace.Unindent();
            SpatialTrace.Indent("Stop points");
            SpatialTrace.SetFillColor(Colors.Violet);
            foreach (var kvp in geogListStopPoints)
            {
                SpatialTrace.TraceGeometry(kvp.Value, kvp.Key, kvp.Key);
            }
            SpatialTrace.Unindent();

            SpatialTrace.ShowDialog();
            SpatialTrace.Disable();
        }

        #region Conversion helpers

        private SqlGeography FromCoordToGeography(Coord coord)
        {
            return SqlGeography.Point(double.Parse(coord.Lat, CultureInfo.InvariantCulture), double.Parse(coord.Lon, CultureInfo.InvariantCulture), 4326);
        }
        private SqlGeometry FromCoordToGeometry(Coord coord)
        {
            return SqlGeometry.Point(double.Parse(coord.Lon, CultureInfo.InvariantCulture), double.Parse(coord.Lat, CultureInfo.InvariantCulture), 4326);
        }
        private SqlGeometry FromCoordToGeometry2154(Coord coord)
        {
            return FromCoordToGeometry(coord).ReprojectTo(2154);
        }

        #endregion
    }
}
