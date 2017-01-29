
using Microsoft.SqlServer.Types;
using NavitiaSharp;
using Newtonsoft.Json;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        const string DATA_DIR = @"..\..\..\Data\SNCF";

        [STAThread()]
        static void Main(string[] args)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            SncfRepository repo = new SncfRepository(DATA_DIR, 1000);

            ShowStopAreasOnMap(repo);

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            // Warning : this does not respect API rules. Use at your own risk
            //repo.SaveStaticData();
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");



            string str2Find = "MEYRARGUES";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "AIX-EN-PROVENCE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "MARSEILLE";
            repo.TestQueryWithStopName(str2Find);

            string idToFind = "stop_area:OCE:SA:87319012";
            repo.TestQueryWithId(idToFind);



        }

        private static void ShowStopAreasOnMap(SncfRepository repo)
        {
            List<SqlGeography> geogList500 = new List<SqlGeography>();
            using (SqlConnection con = new SqlConnection("Data Source=ASUS;Initial Catalog=Sncf;Integrated Security=True"))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT [ID],[NATURE],[ENERGIE],[geom4326].STAsBinary() FROM TRONCON_VOIE_FERREE_500_4326", con))
                {
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



            List<SqlGeography> geogList120 = new List<SqlGeography>();
            using (SqlConnection con = new SqlConnection("Data Source=ASUS;Initial Catalog=Sncf;Integrated Security=True"))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT [geom4326].STAsBinary() FROM TRONCON_VOIE_FERREE_120_4326", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SqlGeography geog = SqlGeography.STGeomFromWKB(reader.GetSqlBytes(0), 4326);
                            geogList120.Add(geog);
                        }
                    }
                }
            }

            Dictionary<string, SqlGeography> geogListStopAreas = new Dictionary<string, SqlGeography>();
            foreach (var sp in repo.DataPack.StopAreas)
            {
                geogListStopAreas.Add(sp.Name + " " + sp.Id, SqlGeography.Point(double.Parse(sp.Coord.Lat,CultureInfo.InvariantCulture), double.Parse(sp.Coord.Lon, CultureInfo.InvariantCulture), 4326));
            }
            Dictionary<string, SqlGeography> geogListStopPoints = new Dictionary<string, SqlGeography>();
            foreach (var sp in repo.DataPack.StopPoints)
            {
                geogListStopPoints.Add(sp.Name + " " + sp.Id, SqlGeography.Point(double.Parse(sp.Coord.Lat, CultureInfo.InvariantCulture), double.Parse(sp.Coord.Lon, CultureInfo.InvariantCulture), 4326));
            }




            SpatialTrace.Enable();
            SpatialTrace.SetLineColor(Colors.Blue);
            SpatialTrace.TraceGeometry(geogList500, "Lignes 500", "Lignes 500");

            SpatialTrace.SetLineColor(Colors.Red);
            SpatialTrace.TraceGeometry(geogList120, "Lignes 120", "Lignes 120");


            SpatialTrace.Indent("Stop areas");
            SpatialTrace.SetLineColor(Colors.Green);
            foreach(var kvp in geogListStopAreas)
            {
                SpatialTrace.TraceGeometry(kvp.Value, kvp.Key,kvp.Key);
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


    }
}
