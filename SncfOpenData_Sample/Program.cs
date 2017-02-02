
using Microsoft.SqlServer.Types;
using NavitiaSharp;
using Newtonsoft.Json;
using SncfOpenData.Model;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SncfOpenData
{

    class Program
    {
        const string DATA_DIR_SNCF = @"..\..\..\Data\SNCF";
        const string DATA_DIR_IGN = @"..\..\..\Data\IGN";
        static string CONN_STRING;

        [STAThread()]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(DATA_DIR_IGN));
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            CONN_STRING = ConfigurationManager.ConnectionStrings["IGNData"].ConnectionString;

            SncfRepository repo = new SncfRepository(DATA_DIR_SNCF, 1000);

            var areasIgn = repo.LoadSavedDataList<StopAreaIGN>("stopAreasIgn.json"));

            areasIgn = MatchStopAreasWithIGNNodes(repo, null); // 1st pass
            areasIgn = MatchStopAreasWithIGNNodes(repo, areasIgn);// 2nd pass
            

            var json = JsonConvert.SerializeObject(areasIgn, Formatting.Indented);
            File.WriteAllText(Path.Combine(DATA_DIR_SNCF, "stopAreasIgn.json"), json);

            //select N.ID_RTE500 As ID_NOEUD
		          //  ,T.ID_RTE500 as ID_TRONCON
            //from NOEUD_FERRE_2154 N
            //left
            //join TRONCON_VOIE_FERREE_2154 T

            //on N.geom2154.STIntersects(T.geom2154) = 1
            //order by ID_NOEUD


            //ShowStopAreasOnMap(repo, "POLYGON((5.2734375 43.259580971072275,5.41351318359375 43.1614915129406,5.4986572265625 43.295574211963746,5.5810546875 43.42936191764414,5.90789794921875 43.57678451504994,5.877685546875 43.74766111392921,5.88043212890625 43.86064850339098,5.62225341796875 43.75559702541283,5.4327392578125 43.670230832122314,5.27069091796875 43.58474304793296,5.23773193359375 43.431356514362626,5.2734375 43.259580971072275))");

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            // Warning : this does not respect API rules. Use at your own risk
            //repo.SaveStaticData();
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            //repo.GetAndSavedRelatedData<Line, StopArea>(repo.Api, repo.DataPack.Lines, "lines/{id}/stop_areas", DATA_DIR_SNCF, "lines.stop_areas.json");
            //repo.GetAndSavedRelatedData<Route, StopArea>(repo.Api, repo.DataPack.Routes, "routes/{id}/stop_areas", DATA_DIR_SNCF, "routes.stop_areas.json");

            // TODO : get all route stop areas
            // we can then map to geo network with a little Dijkstra for each route
            // goal : for each network line, get all connected routes and lines 

            string idStopArea_Meyrargues = "stop_area:OCE:SA:87751370";
            string idStopArea_MarseilleStCharles = "stop_area:OCE:SA:87751008";
            string idStopArea_AixTgv = "stop_area:OCE:SA:87319012";

            var stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_Meyrargues);
            stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_MarseilleStCharles);
            stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_AixTgv);


            repo.TestQueryWithStopAreaId(idStopArea_Meyrargues);
            repo.TestQueryWithStopAreaId(idStopArea_AixTgv);

            string str2Find = "MEYRARGUES";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "AIX-EN-PROVENCE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "MARSEILLE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "NICE";
            repo.TestQueryWithStopName(str2Find);

        }




        private static void MatchLinesWithGeom(SncfRepository repo)
        {
            // Work in L93 proj : use of more funcs on geometry types, and distance calculations done in meters
            /****** Script de la commande SelectTopNRows à partir de SSMS  ******/


            // Full db read
            Dictionary<int, SqlGeometry> geomList2154 = new Dictionary<int, SqlGeometry>();

            using (SqlConnection con = new SqlConnection(CONN_STRING))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT ID_RTE500, geom2154.STAsBinary() FROM[dbo].[TRONCON_VOIE_FERREE_2154]", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = (int)reader[0];
                            SqlGeometry geom2154 = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(1), 2154);
                            geomList2154.Add(id, geom2154);
                        }
                    }
                }
            }

            Dictionary<int, SqlGeometry> geomNoeuds2154 = new Dictionary<int, SqlGeometry>();
            Dictionary<int, string> geomNoeuds2154NAme = new Dictionary<int, string>();

            using (SqlConnection con = new SqlConnection(CONN_STRING))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT geom2154.STAsBinary(),ID_RTE500,TOPONYME FROM [dbo].[NOEUD_FERRE_2154] WHERE NATURE IN ('Gare de voyageurs','Gare de voyageurs et de fret')", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SqlGeometry geom2154 = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(0), 2154);
                            int id = (int)reader[1];
                            string name = reader[2].ToString();
                            geomNoeuds2154.Add(id, geom2154);
                            geomNoeuds2154NAme.Add(id, name);
                        }
                    }
                }
            }

            //foreach (StopArea area in repo.DataPack.StopAreas)
            List<StopAreaIGN> stopAreasIgn = new List<StopAreaIGN>();
            Parallel.ForEach(repo.DataPack.StopAreas, area =>
            {
                StopAreaIGN areaIgn = new StopAreaIGN { StopAreaId = area.Id };

                var closestIgnPoints = from geom in geomNoeuds2154
                                       let area2154 = FromCoordToGeometry2154(area.Coord)
                                       let dist = geom.Value.STDistance(area2154).Value
                                       where dist < 500
                                       orderby dist
                                       select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = geom };

                if (closestIgnPoints.Any())
                {
                    var res = closestIgnPoints.First();
                    Trace.TraceInformation($"{area.Name}: point {geomNoeuds2154NAme[res.IGNObject.Key]} at {(int)Math.Round(res.Distance, 0)}");
                    areaIgn.IdNoeud = res.IGNObject.Key;
                    areaIgn.NomNoeud = geomNoeuds2154NAme[res.IGNObject.Key];
                    areaIgn.DistanceNoeud = res.Distance;

                }
                else
                {
                    Trace.TraceInformation($"{area.Name}: not point found.");
                }
                stopAreasIgn.Add(areaIgn);

            }
            );

            var json = JsonConvert.SerializeObject(stopAreasIgn, Formatting.Indented);
            File.WriteAllText(Path.Combine(DATA_DIR_SNCF, "stopAreasIgn.json"), json);
            SpatialTrace.Enable();

            foreach (Line line in repo.DataPack.Lines)
            {
                List<StopArea> stopAreas = repo.DataPack.LinesStopAreas.Where(kvp => kvp.Key == line.Id)
                                                          .Select(kvp => kvp.Value)
                                                          .FirstOrDefault();

                if (stopAreas != null)
                {
                    foreach (StopArea area in stopAreas)
                    {
                        //SqlGeometry area2154 = FromCoordToGeometry2154(area.Coord);


                        var closestIgnRoutes = from geom in geomList2154
                                               let area2154 = FromCoordToGeometry2154(area.Coord)
                                               let dist = geom.Value.STDistance(area2154).Value
                                               where dist < 500
                                               orderby dist
                                               select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = geom };

                        var closestIgnPoints = from geom in geomNoeuds2154
                                               let area2154 = FromCoordToGeometry2154(area.Coord)
                                               let dist = geom.Value.STDistance(area2154).Value
                                               where dist < 500
                                               orderby dist
                                               select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = geom };

                        var results = closestIgnRoutes.ToList();

                        var resultsPoints = closestIgnPoints.ToList();

                        SpatialTrace.Indent(area.Name);
                        SpatialTrace.SetFillColor(Colors.Green);
                        SpatialTrace.TraceGeometry(FromCoordToGeometry2154(area.Coord).STBuffer(100), area.Name, area.Name);


                        foreach (var result in results)
                        {
                            int idRte500 = result.IGNObject.Key;

                            // line
                            SpatialTrace.TraceGeometry(result.IGNObject.Value, result.IGNObject.Key + " " + result.Distance.ToString(), result.IGNObject.Key + " " + result.Distance.ToString());

                        }
                        SpatialTrace.SetFillColor(Colors.Blue);
                        foreach (var result in resultsPoints)
                        {
                            int idRte500 = result.IGNObject.Key;

                            // point
                            SpatialTrace.TraceGeometry(result.IGNObject.Value.STBuffer(100), result.IGNObject.Key + " " + result.Distance.ToString(), result.IGNObject.Key + " " + result.Distance.ToString());

                        }


                        SpatialTrace.Unindent();
                    }
                }

                SpatialTrace.ShowDialog();
            }


            SpatialTrace.Disable();
        }

        private static List<StopAreaIGN> MatchStopAreasWithIGNNodes(SncfRepository repo, List<StopAreaIGN> stopAreasIgn)
        {
            // Work in L93 proj : use of more funcs on geometry types, and distance calculations done in meters

            // Full db read
            #region Full DB read

            Dictionary<int, SqlGeometry> DBgeomList2154 = new Dictionary<int, SqlGeometry>();

            using (SqlConnection con = new SqlConnection(CONN_STRING))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT ID_RTE500, geom2154.STAsBinary() FROM[dbo].[TRONCON_VOIE_FERREE_2154]", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = (int)reader[0];
                            SqlGeometry geom2154 = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(1), 2154);
                            DBgeomList2154.Add(id, geom2154);
                        }
                    }
                }
            }

            Dictionary<int, SqlGeometry> DBgeomNoeuds2154 = new Dictionary<int, SqlGeometry>();
            Dictionary<int, string> DBgeomNoeuds2154NAme = new Dictionary<int, string>();

            using (SqlConnection con = new SqlConnection(CONN_STRING))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT geom2154.STAsBinary(),ID_RTE500,TOPONYME FROM [dbo].[NOEUD_FERRE_2154] WHERE NATURE IN ('Gare de voyageurs','Gare de voyageurs et de fret')", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SqlGeometry geom2154 = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(0), 2154);
                            int id = (int)reader[1];
                            string name = reader[2].ToString();
                            DBgeomNoeuds2154.Add(id, geom2154);
                            DBgeomNoeuds2154NAme.Add(id, name);
                        }
                    }
                }
            }
            #endregion

            if (stopAreasIgn == null)
            {
                #region 1st pass : match within 500 meters (CPU intensive)

                stopAreasIgn = new List<StopAreaIGN>();
                Parallel.ForEach(repo.DataPack.StopAreas, area =>
                {
                    StopAreaIGN areaIgn = new StopAreaIGN { StopAreaId = area.Id };

                    var closestIgnPoints = from geom in DBgeomNoeuds2154
                                           let area2154 = FromCoordToGeometry2154(area.Coord)
                                           let dist = geom.Value.STDistance(area2154).Value
                                           where dist < 500
                                           orderby dist
                                           select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = geom };

                    if (closestIgnPoints.Any())
                    {
                        var res = closestIgnPoints.First();
                        Trace.TraceInformation($"{area.Name}: point {DBgeomNoeuds2154NAme[res.IGNObject.Key]} at {(int)Math.Round(res.Distance, 0)}");
                        areaIgn.IdNoeud = res.IGNObject.Key;
                        areaIgn.NomNoeud = DBgeomNoeuds2154NAme[res.IGNObject.Key];
                        areaIgn.DistanceNoeud = res.Distance;

                    }
                    else
                    {
                        Trace.TraceInformation($"{area.Name}: not point found.");
                    }
                    stopAreasIgn.Add(areaIgn);

                }
                );

                #endregion

            }
            else
            {
                #region 2nd pass : for non matched, match within 30000 meters (CPU intensive)

                var stopAreasIgn_NonMatched = stopAreasIgn.Where(s => s.HasIGNMatch == false);
                //Parallel.ForEach(repo.DataPack.StopAreas, area =>
                foreach (var stopAreaIgn in stopAreasIgn_NonMatched)
                {
                    StopArea area = repo.DataPack.StopAreas.Where(s => s.Id == stopAreaIgn.StopAreaId).Single();
                    var closestIgnPoints = from geom in DBgeomNoeuds2154
                                           let area2154 = FromCoordToGeometry2154(area.Coord)
                                           let dist = geom.Value.STDistance(area2154).Value
                                           where dist < 30000
                                           orderby dist
                                           select new { Distance = dist, StopArea = area, CoordL93 = area2154, IGNObject = geom };

                    if (closestIgnPoints.Any())
                    {
                        var res = closestIgnPoints.First();
                        if (DBgeomNoeuds2154NAme[res.IGNObject.Key].ToUpper().Trim() == area.Name.ToUpper().Trim())
                        {
                            //Trace.TraceInformation($"{area.Name}: point {DBgeomNoeuds2154NAme[res.IGNObject.Key]} at {(int)Math.Round(res.Distance, 0)}");
                            stopAreaIgn.IdNoeud = res.IGNObject.Key;
                            stopAreaIgn.NomNoeud = DBgeomNoeuds2154NAme[res.IGNObject.Key];
                            stopAreaIgn.DistanceNoeud = res.Distance;
                        }
                        else
                        {
                            Trace.TraceWarning($"WARN : {area.Name} does not match with point {DBgeomNoeuds2154NAme[res.IGNObject.Key]}. Distance : {(int)Math.Round(res.Distance, 0)}");

                        }
                    }
                    else
                    {
                       // Trace.TraceInformation($"{area.Name}: not point found.");
                    }

                }
                //);

                #endregion
            }

            return stopAreasIgn;
        }



        private static void ShowStopAreasOnMap(SncfRepository repo, string wkt = null)
        {

            SqlGeography polyQuery = wkt == null ? null : SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);

            List<SqlGeography> geogList500 = new List<SqlGeography>();

            using (SqlConnection con = new SqlConnection(CONN_STRING))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT [ID],[NATURE],[ENERGIE],[geom4326].STAsBinary() FROM TRONCON_VOIE_FERREE_4326", con))
                {
                    AddSpatialIntersectionPredicate(com, "geom4326", polyQuery);

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SqlGeography geog = SqlGeography.STGeomFromWKB(reader.GetSqlBytes(3), 4326);
                            geogList500.Add(geog);
                        }
                    }
                }
            }



            //List<SqlGeography> geogList120 = new List<SqlGeography>();
            //using (SqlConnection con = new SqlConnection(connectionString))
            //{
            //    con.Open();
            //    using (SqlCommand com = new SqlCommand("SELECT [geom4326].STAsBinary() FROM TRONCON_VOIE_FERREE_4326", con))
            //    {
            //        AddSpatialIntersectionPredicate(com, "geom4326", polyQuery);
            //        using (SqlDataReader reader = com.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                SqlGeography geog = SqlGeography.STGeomFromWKB(reader.GetSqlBytes(0), 4326);
            //                geogList120.Add(geog);
            //            }
            //        }
            //    }
            //}

            Dictionary<string, SqlGeography> geogListStopAreas = new Dictionary<string, SqlGeography>();
            IEnumerable<StopArea> stopAreas = repo.DataPack.StopAreas;
            if (polyQuery != null)
            {
                stopAreas = stopAreas.Where(s => FromCoordToGeography(s.Coord).STIntersects(polyQuery).IsTrue);
            }
            foreach (var sp in stopAreas)
            {
                geogListStopAreas.Add(sp.Name + " " + sp.Id, FromCoordToGeography(sp.Coord));
            }
            Dictionary<string, SqlGeography> geogListStopPoints = new Dictionary<string, SqlGeography>();
            IEnumerable<StopPoint> stopPoints = repo.DataPack.StopPoints;
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
            foreach (var g in geogList500)
            {
                if (i % 2 == 0)
                {
                    SpatialTrace.SetLineColor(Colors.Blue);
                }
                else
                {
                    SpatialTrace.SetLineColor(Colors.Red);
                }
                SpatialTrace.TraceGeometry(g, "Lignes 500", "Lignes 500");
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

        private static SqlGeography FromCoordToGeography(Coord coord)
        {
            return SqlGeography.Point(double.Parse(coord.Lat, CultureInfo.InvariantCulture), double.Parse(coord.Lon, CultureInfo.InvariantCulture), 4326);
        }
        private static SqlGeometry FromCoordToGeometry(Coord coord)
        {
            return SqlGeometry.Point(double.Parse(coord.Lon, CultureInfo.InvariantCulture), double.Parse(coord.Lat, CultureInfo.InvariantCulture), 4326);
        }
        private static SqlGeometry FromCoordToGeometry2154(Coord coord)
        {
            return FromCoordToGeometry(coord).ReprojectTo(2154);
        }

        private static void AddSpatialIntersectionPredicate(SqlCommand com, string spatialField, SqlGeography polyQuery)
        {
            if (polyQuery == null)
                return;
            var param = com.Parameters.Add("@g", SqlDbType.Udt);
            param.Value = polyQuery;
            param.UdtTypeName = "geography";

            com.CommandText += $" WHERE {spatialField}.STIntersects(@g) = 1";
        }


    }
}
