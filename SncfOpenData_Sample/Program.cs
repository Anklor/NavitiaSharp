
using Microsoft.SqlServer.Types;
using NavitiaSharp;
using Newtonsoft.Json;
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

        [STAThread()]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(DATA_DIR_IGN));
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            SncfRepository repo = new SncfRepository(DATA_DIR_SNCF, 1000);

            ShowStopAreasOnMap(repo, "POLYGON((5.2734375 43.259580971072275,5.41351318359375 43.1614915129406,5.4986572265625 43.295574211963746,5.5810546875 43.42936191764414,5.90789794921875 43.57678451504994,5.877685546875 43.74766111392921,5.88043212890625 43.86064850339098,5.62225341796875 43.75559702541283,5.4327392578125 43.670230832122314,5.27069091796875 43.58474304793296,5.23773193359375 43.431356514362626,5.2734375 43.259580971072275))");

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            // Warning : this does not respect API rules. Use at your own risk
            //repo.SaveStaticData();
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            string idStopArea_Meyrargues = "stop_area:OCE:SA:87751370";
            string idStopArea_MarseilleStCharles = "stop_area:OCE:SA:87751008";
            string idStopArea_AixTgv = "stop_area:OCE:SA:87319012";
            repo.TestQueryWithStopAreaId(idStopArea_Meyrargues);
            repo.TestQueryWithStopAreaId(idStopArea_AixTgv);

            string str2Find = "MEYRARGUES";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "AIX-EN-PROVENCE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "MARSEILLE";
            repo.TestQueryWithStopName(str2Find);

           


        }

        private static void ShowStopAreasOnMap(SncfRepository repo, string wkt = null)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["IGNData"].ConnectionString;
            SqlGeography polyQuery = wkt == null ? null : SqlGeography.STGeomFromText(new SqlChars(new SqlString(wkt)), 4326);

            List<SqlGeography> geogList500 = new List<SqlGeography>();
            
            using (SqlConnection con = new SqlConnection(connectionString))
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
                stopAreas = stopAreas.Where(s => FromCoordToGeom(s.Coord).STIntersects(polyQuery).IsTrue);
            }
            foreach (var sp in stopAreas)
            {
                geogListStopAreas.Add(sp.Name + " " + sp.Id, FromCoordToGeom(sp.Coord));
            }
            Dictionary<string, SqlGeography> geogListStopPoints = new Dictionary<string, SqlGeography>();
            IEnumerable<StopPoint> stopPoints = repo.DataPack.StopPoints;
            if (polyQuery != null)
            {
                stopPoints = stopPoints.Where(s => FromCoordToGeom(s.Coord).STIntersects(polyQuery).IsTrue);
            }
            foreach (var sp in stopPoints)
            {
                geogListStopPoints.Add(sp.Name + " " + sp.Id, FromCoordToGeom(sp.Coord));
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
            SpatialTrace.SetLineColor(Colors.Green);
            foreach (var kvp in geogListStopAreas)
            {
                SpatialTrace.TraceGeometry(kvp.Value, kvp.Key, kvp.Key);
            }

            SpatialTrace.Unindent();
            SpatialTrace.Indent("Stop points");
            SpatialTrace.SetLineColor(Colors.Violet);
            foreach (var kvp in geogListStopPoints)
            {
                SpatialTrace.TraceGeometry(kvp.Value, kvp.Key, kvp.Key);
            }
            SpatialTrace.Unindent();

            SpatialTrace.ShowDialog();
            SpatialTrace.Disable();
        }

        private static SqlGeography FromCoordToGeom(Coord coord)
        {
            return SqlGeography.Point(double.Parse(coord.Lat, CultureInfo.InvariantCulture), double.Parse(coord.Lon, CultureInfo.InvariantCulture), 4326);
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
