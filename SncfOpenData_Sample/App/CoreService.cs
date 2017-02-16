using Microsoft.SqlServer.Types;
using GraphCollection;
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
using System.Text;
using System.IO;
using SncfOpenData.Topology;
using System.Collections.Concurrent;

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
            var nodes = _ignRepo.GetAllNoeuds_Lambert93();
            var troncons = _ignRepo.GetAllTroncons_Lambert93();
            PathFinder pathfinder = new PathFinder(troncons, nodes);

            IEnumerable<Route> routes = _sncfRepo.Routes;
            // debug test route
            //routes = routes.Take(1);
            //routes = routes.Where(r => r.Id == "route:OCE:343-TrainTER-87751008-87763607");
            routes = routes.Where(r => r.Id == "route:OCE:10-TrainTER-87473009-87473181");
            //routes = routes.Where(r => r.Line.Id == "line:OCE:343");
            Trace.Listeners.Add(new ConsoleTraceListener());


            ParallelQuery<RoutePathResult> results = routes.AsParallel().Select(route => CalculateRoutePath(route, pathfinder));
            using (StreamWriter sw = new StreamWriter("matchroutes.txt", false))
            {
                foreach (var result in results)
                {
                    Route route = result.Route;
                    sw.WriteLine("---------------------------------");
                    sw.WriteLine("-- Route " + route.Id);
                    sw.WriteLine("--" + route.Name + " TO " + route.Direction.Name);
                    sw.WriteLine(IdsAsSQLInClause(result.StopAreasNode.Values.Select(n => n.Id)));

                    try
                    {

                        sw.WriteLine("--");
                        if (result.ResultTroncons.Any())
                        {
                            var tronconsIds = "(" + String.Join<int>("),(", result.ResultTroncons.Select(p => p.Id)) + ")"; // check path clause

                            sw.WriteLine(tronconsIds);
                        }
                        else
                        {
                            sw.WriteLine("No route found !");
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine("Error: " + ex.ToString());
                        Trace.TraceError("Error: " + ex.Message);
                    }


                    sw.Flush();
                    Trace.TraceInformation($"MatchRoutesWithTronconsIGN: Done.");


                    // TODO : Dijkstra for all troncons within envelope of all line stop areas
                    //FindPath(topologyByTroncon, topologyByNode, tronconsInRoute.ToDictionary(t => t.Id, t => t), ignNodes.First(), ignNodes.Last());
                }
            }
        }

        public RoutePathResult CalculateRoutePath(Route route, PathFinder pathfinder)
        {
            Line line = _sncfRepo.Lines.First(l => l.Id == route.Line.Id);
            RoutePathResult result = new RoutePathResult(route, line);

            Trace.TraceInformation($"MatchRoutesWithTronconsIGN: route {route.Name}...");

            // How to get proper order for stop areas ?
            // Let's look at route schedules
            List<RouteSchedule> schedules = _sncfRepo.GetRouteSchedules(route, false);
            //Debug.Assert(schedules.Count == 1, $"Zero or more than one schedule for route {route.Name}");
            result.Schedule = schedules.FirstOrDefault();
            if (result.Schedule == null)
            {
                Trace.TraceWarning($"No schedule found.");
            }
            else if (!result.Schedule.Table.WithSchedule)
            {
                Trace.TraceWarning($"No time tables for schedule.");
            }
            else
            {
                List<StopArea> stopareas = null;
                _sncfRepo.RoutesStopAreas.TryGetValue(route.Id, out stopareas);
                if (stopareas == null || stopareas.Count == 0)
                {
                    Trace.TraceWarning($"No stop areas found.");
                }
                else
                {

                    stopareas = FilterAndSortStopAreas(stopareas, result.Schedule);
                    result.StopAreas = stopareas;

                    Dictionary<StopArea, int> ignNodes = GetStopAreaIgnNodes(stopareas);
                    result.StopAreasNode = ignNodes.ToDictionary(kvp => kvp.Key, kvp => pathfinder.Nodes[kvp.Value]);
                    var nodesIds = "(" + String.Join<int>("),(", ignNodes.Values) + ")"; // insert clause
                    try
                    {
                        result.ResultTroncons = pathfinder.FindPath(ignNodes.Values, line.CommercialMode, 5000);
                    }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                        Trace.TraceError("Error: " + ex.Message);
                    }
                }
            }

            return result;
        }

        private Dictionary<StopArea, int> GetStopAreaIgnNodes(List<StopArea> stopareas)
        {
            return stopareas.ToDictionary(sa => sa, sa => _sncfRepo.IgnNodeByStopArea[sa.Id]);
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

        private string IdsAsSQLInClause(IEnumerable<int> ids)
        {
            return "(" + string.Join("),(", ids) + ")"; // insert clause
        }

        #endregion

        #region Sort helpers

        private List<StopArea> FilterAndSortStopAreas(List<StopArea> stopareas, RouteSchedule routeSchedule)
        {
            List<StopArea> result = stopareas;
            try
            {
                if (routeSchedule.Table.WithSchedule)
                {
                    // sort and filter using schedule dates
                    Dictionary<StopArea, DateTime> firstSchedule = routeSchedule.Table.Rows.Select(r => new { StopArea = r.StopPoint.StopArea, DateTime = r.DateTimes.First() })
                                                                                          .Where(a => a.DateTime.DateTime != default(DateTime))
                                                                                          .OrderBy(a => a.DateTime.DateTime)
                                                                                          .ToDictionary(a => a.StopArea, a => a.DateTime.DateTime);

                    result = (from sa in stopareas
                              join s in firstSchedule on sa equals s.Key
                              orderby s.Value
                              select sa).ToList();
                }
                else
                {
                    // No schedule dates. stopAreas must be then taken from route schedule, in schedule order
                    result = (from row in routeSchedule.Table.Rows
                              let sp = _sncfRepo.StopPoints.Where(stopPoint => stopPoint.Id == row.StopPoint.Id).First()
                              select sp.StopArea).ToList();


                }
                return result;
            }
            catch (Exception)
            {
                result = stopareas;
            }
            return result;
        }

        private int GetStopAreaIndexWithoutSchedule(StopArea sa, StopArea origin, StopArea dest)
        {
            return sa.Id == origin.Id ? 0
                : sa.Id == dest.Id ? 2
                : 1;
        }



        #endregion
    }
}
