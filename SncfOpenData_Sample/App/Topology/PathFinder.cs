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

            SqlGeometry geom = GetNodesGeometryAggregate(ignNodes.Values, 30);
            geom = geom.STEnvelope().STBuffer(bufferAroundPoints).STEnvelope();
            var tronconsInRoute = FilterTronconsByGeometry(_troncons, geom).ToList();
            var nodesInRoute = FilterNodesByGeometry(_nodes, geom).ToList();

            // Generate topology
            var topology = Topology.Compute(tronconsInRoute, ignNodes.Values.ToList());

            // Launch path finding
            var troncons = FindPath_GraphCollection(topology, ignNodes);
            return troncons;
        }

        /// <summary>
        /// Creates a graph representing network railway, and performs a Dijkstra search to find best possible route.
        /// This route must :
        ///     - Pass by all checkpoints (stop points)
        ///     - Must follow railway network (no hard turns) => lines crossing each other are in real life bridges or tunnels.
        ///         Those intersections must be treated as distinct nodes connecting only heading-compatible edges
        /// </summary>
        /// <param name="topology"></param>
        /// <param name="checkpoints"></param>
        /// <returns></returns>
        private List<Troncon> FindPath_GraphCollection(Topology topology, Dictionary<int, Noeud> stopPoints)
        {
            IEnumerable<GraphNode<int>> graph = GenerateGraph(topology, stopPoints);

            IList<GraphNode<int>> path = FindShortestPath(topology, graph, stopPoints);

            List<Troncon> troncons = TransformPathToEdgeList(topology, path);
            return troncons;
        }

        #region FindPath_GraphCollection

        /// <summary>
        /// Generates graph
        /// Ensure heading-compatible edges are connected
        /// </summary>
        /// <param name="topology"></param>
        /// <param name="stopPoints"></param>
        /// <returns></returns>
        private IEnumerable<GraphNode<int>> GenerateGraph(Topology topology, Dictionary<int, Noeud> stopPoints)
        {
            Dictionary<int, GraphNode<int>> graphNodes = topology.TopoNodes.ToDictionary(node => node.Key, node => new GraphNode<int>(node.Key));

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

            return graphNodes.Values;
        }

        private IList<GraphNode<int>> FindShortestPath(Topology topology, IEnumerable<GraphNode<int>> graph, Dictionary<int, Noeud> stopPoints)
        {
            TopoNode topoNodeStart = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(stopPoints.First().Value.Geometry).IsTrue).Single().Value;
            TopoNode topoNodeEnd = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(stopPoints.Last().Value.Geometry).IsTrue).Single().Value;

            GraphNode<int> start = graph.Where(n => n.Value == topoNodeStart.Id).Single();
            GraphNode<int> end = graph.Where(n => n.Value == topoNodeEnd.Id).Single();


            var dijkstra = new Dijkstra<int>(graph);
            var path = dijkstra.FindShortestPathBetween(start, end);

            return path;
        }

        private List<Troncon> TransformPathToEdgeList(Topology topology, IList<GraphNode<int>> path)
        {
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

        #endregion

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

        private SqlGeometry GetNodesGeometryAggregate(IEnumerable<Noeud> nodes, double buffer)
        {
            SqlGeometry geom = SqlTypesExtensions.PointEmpty_SqlGeometry(2154);
            foreach (var pointGeom in nodes)
            {
                geom = geom.STUnion(buffer == 0d ? pointGeom.Geometry : pointGeom.Geometry.STBuffer(0));
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
