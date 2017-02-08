using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SncfOpenData.IGN.Model;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using SncfOpenData.App.Topology;
using GraphCollection;

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
        internal List<Troncon> FindPath(HashSet<int> checkpoints, int bufferAroundPoints = 5000)
        {
            Dictionary<int, Noeud> ignNodes = FilterNodesById(_nodes, checkpoints);

            SqlGeometry geom = GetNodesGeometryAggregate(ignNodes.Values);
            geom = geom.STEnvelope().STBuffer(bufferAroundPoints).STEnvelope();

            var tronconsInRoute = FilterTronconsByGeometry(_troncons, geom).ToList();
            var nodesInRoute = FilterNodesByGeometry(_nodes, geom).ToList();

            // Generate topology
            var topology = Topology.Compute(tronconsInRoute, ignNodes.Values.ToList());

            //FindPath(topology, ignNodes);
            var troncons = FindPath_GraphCollection(topology, ignNodes);
            return troncons;
        }

        private List<Troncon> FindPath_GraphCollection(Topology topology, Dictionary<int, Noeud> checkpoints)
        {
            Dictionary<int, GraphNode<int>> graphNodes = topology.TopoNodes.Select(kvp => kvp.Key).ToDictionary(k => k, k => new GraphNode<int>(k));
            var topoNodeStart = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(checkpoints.First().Value.Geometry).IsTrue).Single().Value;
            var topoNodeEnd = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(checkpoints.Last().Value.Geometry).IsTrue).Single().Value;
            foreach (var node in topology.TopoNodes)
            {
                // find nodes directly reachable from node
                Dictionary<TopoTroncon, TopoNode> accessibleNodes = GetAccessibleNodes(topology, node.Value);
                foreach (var closeNode in accessibleNodes)
                {
                    GraphNode<int> gNodeStart = graphNodes[node.Value.Id];
                    GraphNode<int> gNodeEnd = graphNodes[closeNode.Value.Id];
                    gNodeStart.AddNeighbour(gNodeEnd, (int)closeNode.Key.Geometry.STLength().Value);
                }
            }

            var dijkstra = new Dijkstra<int>(graphNodes.Values);
            var path = dijkstra.FindShortestPathBetween(graphNodes[topoNodeStart.Id], graphNodes[topoNodeEnd.Id]);

            List<Troncon> pathInTroncons = new List<Troncon>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                pathInTroncons.Add(GetTronconBetweenNodes(topology, current.Value, next.Value));
            }

            return pathInTroncons;
        }

        private Troncon GetTronconBetweenNodes(Topology topology, int nodeIdFrom, int nodeIdTo)
        {
            Troncon trn = topology.TopoTroncons.Where(kvp => kvp.Value.IdNodes.Contains(nodeIdFrom) && kvp.Value.IdNodes.Contains(nodeIdTo))
                                 .Select(t => topology.Troncons.First(baseTrn => baseTrn.Id == t.Value.IdTroncon))
                                 .Single();
            return trn;
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
                //List<TopoNode> accessibleNodes = GetAccessibleNodes(topology, topoNodeStart);


            }
        }

        // TODO : check in troncon has compatible headings
        private Dictionary<TopoTroncon, TopoNode> GetAccessibleNodes(Topology topology, TopoNode topoNode)
        {
            Dictionary<TopoTroncon, TopoNode> nodesByTroncon = new Dictionary<TopoTroncon, TopoNode>();

            if (topoNode.IdTroncons.Any())
            {
                foreach (var idTroncon in topoNode.IdTroncons)
                {
                    if (topology.TopoTroncons.ContainsKey(idTroncon))
                    {
                        TopoTroncon trn = topology.TopoTroncons[idTroncon];
                        int idNode = trn.IdNodes.Except(new int[] { topoNode.Id }).SingleOrDefault();

                        nodesByTroncon[trn] = topology.TopoNodes[idNode];
                    }
                }
            }
            return nodesByTroncon;
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
        // Filter nodes catalog and returns sub catalog in the same order as keys
        private Dictionary<int, Noeud> FilterNodesById(Dictionary<int, Noeud> nodes, HashSet<int> keys)
        {
            Dictionary<int, Noeud> filteredAndSorted = new Dictionary<int, Noeud>();
            foreach (var nodeId in keys)
            {
                if (nodes.ContainsKey(nodeId))
                {
                    filteredAndSorted[nodeId] = nodes[nodeId];
                }
            }
            return filteredAndSorted;
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
