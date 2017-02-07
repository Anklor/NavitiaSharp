using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SncfOpenData.IGN.Model;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using SncfOpenData.App.Topology;

namespace SncfOpenData
{
    public class PathFinder
    {
        private Dictionary<int, Noeud> _nodes;
        private Dictionary<int, Troncon> _troncons;


        public PathFinder(Dictionary<int, Troncon> troncons, Dictionary<int, Noeud> nodes)
        {
            this._troncons = troncons;
            this._nodes = nodes;
        }

        /// <summary>
        /// Finds path passing by all checkpoints in specified order
        /// </summary>
        /// <param name="checkpoints"></param>
        /// <param name="bufferAroundPoints">Distance around point to minimize network analysis</param>
        internal void FindPath(HashSet<int> checkpoints, int bufferAroundPoints = 5000)
        {
            Dictionary<int, Noeud> ignNodes = FilterNodesById(_nodes, checkpoints);

            SqlGeometry geom = GetNodesGeometryAggregate(ignNodes.Values);
            geom = geom.STEnvelope().STBuffer(bufferAroundPoints).STEnvelope();

            var tronconsInRoute = FilterTronconsByGeometry(_troncons, geom).ToList();
            var nodesInRoute = FilterNodesByGeometry(_nodes, geom).ToList();

            // Generate topology
            var topology = Topology.Compute(tronconsInRoute, ignNodes.Values.ToList());

            FindPath(topology, ignNodes);
        }

        private void FindPath(Topology topology, Dictionary<int, Noeud> checkpoints)
        {
            List<int> keys = checkpoints.Keys.ToList();
            for (int i = 0; i < keys.Count - 1; i++)
            {
                Noeud start = checkpoints[keys[i]];
                Noeud stop = checkpoints[keys[i + 1]];


                var topoNodeStart = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(start.Geometry).IsTrue).Single().Value;
                var topoNodeEnd = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(stop.Geometry).IsTrue).Single().Value;

                // find path
                List<TopoNode> accessibleNodes = GetAccessibleNodes(topology, topoNodeStart);
                

            }
        }

        private List<TopoNode> GetAccessibleNodes(Topology topology, TopoNode topoNode)
        {
            HashSet<int> visitedIds = new HashSet<int>();

            if (topoNode.IdTroncons.Any())
            {
                foreach (var idTroncon in topoNode.IdTroncons)
                {
                    if (topology.TopoTroncons.ContainsKey(idTroncon))
                    {
                        TopoTroncon trn = topology.TopoTroncons[idTroncon];
                        visitedIds.UnionWith(trn.IdNodes);                       
                    }
                }
            }
            return topology.TopoNodes.Where(kvp => visitedIds.Contains(kvp.Key)).Select(v => v.Value).ToList();
        }

        #region Helpers

        private SqlGeometry GetNodesGeometryAggregate(IEnumerable<Noeud> nodes)
        {
            SqlGeometry geom = SqlTypesExtensions.PointEmpty_SqlGeometry(2154);
            foreach (var pointGeom in nodes)
            {
                geom = geom.STUnion(pointGeom.Geometry);
            }
            return geom;
        }

        //private IEnumerable<Noeud> FilterNodesById(Dictionary<int, Noeud> nodes, HashSet<int> keys)
        //      {
        //          return nodes.Where(kvp => keys.Contains(kvp.Key)).Select(kvp => kvp.Value);
        //      }
        private Dictionary<int, Noeud> FilterNodesById(Dictionary<int, Noeud> nodes, HashSet<int> keys)
        {
            return nodes.Where(kvp => keys.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        private IEnumerable<Noeud> FilterNodesByGeometry(Dictionary<int, Noeud> nodes, SqlGeometry geomFilter)
        {
            return nodes.Where(kvp => kvp.Value.Geometry.STIntersects(geomFilter).Value == true)
                            .Select(t => t.Value);
        }
        private IEnumerable<Troncon> FilterTronconsByGeometry(Dictionary<int, Troncon> troncons, SqlGeometry geomFilter)
        {
            return troncons.Where(kvp => kvp.Value.Geometry.STIntersects(geomFilter).Value == true)
                            .Select(t => t.Value);
        }

        #endregion
    }
}
