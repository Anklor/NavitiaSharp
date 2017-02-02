using Microsoft.SqlServer.Types;
using SncfOpenData.IGN.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SqlServerSpatial.Toolkit;

namespace SncfOpenData.IGN
{
    public class IGNRepository
    {
        private readonly string ConnectionString;
        public IGNRepository(string databasePath)
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            ConnectionString = $"Data Source=(localdb)\\v11.0;AttachDbFilename={System.IO.Path.GetFullPath(databasePath)};Integrated Security=True";
        }

        public Dictionary<int, Troncon> GetAllTroncons_Lambert93()
        {
            Dictionary<int, Troncon> troncons2154 = new Dictionary<int, Troncon>();

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT geom2154.STAsBinary(),[ID_RTE500],[NATURE],[ENERGIE],[CLASSEMENT] FROM [dbo].[TRONCON_VOIE_FERREE_2154]", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Troncon trn = new Troncon();
                            trn.Id = (int)Convert.ChangeType(reader["ID_RTE500"], typeof(int));
                            trn.Nature = reader["NATURE"].ToString();
                            trn.Energie = reader["ENERGIE"].ToString();
                            trn.Classement = reader["CLASSEMENT"].ToString();
                            trn.Geometry = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(0), 2154);
                            troncons2154.Add(trn.Id, trn);
                        }
                    }
                }
            }

            return troncons2154;
        }

        public Dictionary<int, Noeud> GetAllNoeuds_Lambert93()
        {
            Dictionary<int, Noeud> noeuds2154 = new Dictionary<int, Noeud>();

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT [geom2154].STAsBinary(),[ID_RTE500],[NATURE],[TOPONYME] /*,[ID_TRONCON],[ID_COMMUNE] */ FROM [dbo].[NOEUD_FERRE_2154]", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Noeud node = new Noeud();
                            node.Geometry = SqlGeometry.STGeomFromWKB(reader.GetSqlBytes(0), 2154);
                            node.Id = (int)Convert.ChangeType(reader["ID_RTE500"], typeof(int));
                            node.Nature = reader["NATURE"].ToString();
                            node.Toponyme = reader["TOPONYME"].ToString();
                            noeuds2154.Add(node.Id, node);
                        }
                    }
                }
            }

            return noeuds2154;
        }

        public Dictionary<int, Noeud> GetAllNoeuds_LatLon(SqlGeography polyQuery)
        {

            SqlGeometry geomQuery = null;
            if (polyQuery.TryToGeometry(out geomQuery))
            {
                geomQuery = geomQuery.ReprojectTo(2154);
                return GetAllNoeuds_Lambert93()
                      .Where(kvp => kvp.Value.Geometry.STIntersects(geomQuery).Value == true)
                      .ToDictionary(kvp => kvp.Key, kvp => ReprojectNoeud(kvp.Value, 4326));
            }
            else
            {
                return GetAllNoeuds_Lambert93()
                    .ToDictionary(kvp => kvp.Key, kvp => ReprojectNoeud(kvp.Value, 4326));
            }

            //using (SqlConnection con = new SqlConnection(CONN_STRING))
            //{
            //    con.Open();
            //    using (SqlCommand com = new SqlCommand("SELECT [ID],[NATURE],[ENERGIE],[geom4326].STAsBinary() FROM TRONCON_VOIE_FERREE_4326", con))
            //    {
            //        AddSpatialIntersectionPredicate(com, "geom4326", polyQuery);

            //        using (SqlDataReader reader = com.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                SqlGeography geog = SqlGeography.STGeomFromWKB(reader.GetSqlBytes(3), 4326);
            //                geogList500.Add(geog);
            //            }
            //        }
            //    }
            //}

        }
        
        private Noeud ReprojectNoeud(Noeud value, int srid)
        {
            value.Geometry = value.Geometry.ReprojectTo(srid);
            return value;
        }
    }
}
