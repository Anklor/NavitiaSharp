using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SncfOpenData.IGN.Model;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;
using GraphCollection;
using System.Diagnostics;
using NavitiaSharp;

namespace SncfOpenData.Topology
{
    public class PathFinder
    {
        private Dictionary<int, Noeud> _nodes; // Network nodes
        private Dictionary<int, Troncon> _troncons; // Edges


        public PathFinder(Dictionary<int, Troncon> troncons, Dictionary<int, Noeud> nodes)
        {
            this._troncons = troncons;
            this._nodes = nodes;
        }

        /// <summary>
        /// Finds path passing by all checkpoints in specified order
        /// </summary>
        /// <param name="stopAreas"></param>
        /// <param name="bufferAroundPoints">Distance around point to minimize network analysis</param>
        internal List<Troncon> FindPath(HashSet<int> stopAreas, CommercialMode commercialMode, int bufferAroundPoints = 5000)
        {
            // get only stop area nodes
            Dictionary<int, Noeud> stopAreasIgnNodes = FilterNodesById(_nodes, stopAreas);

            // get subnetwork convering only all stop areas 
            SqlGeometry geom = GetNodesGeometryAggregate(stopAreasIgnNodes.Values, 30);
            geom = geom.STEnvelope().STBuffer(bufferAroundPoints).STEnvelope();
            var tronconsInRoute = FilterTronconsByGeometry(_troncons, geom);
            tronconsInRoute = FilterTronconsByCommercialMode(tronconsInRoute, commercialMode);
            var nodesInRoute = FilterNodesByGeometry(_nodes, geom).ToList();

            // Generate topology
            var topology = Topology.Compute(tronconsInRoute.ToList(), nodesInRoute);

            // Launch path finding
            var troncons = FindPath_GraphCollection(topology, stopAreasIgnNodes);
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
            if (!graph.Any())
            {
                Trace.TraceWarning("Graph is empty.");
                return new List<Troncon>();
            }
            else
            {
                IList<GraphNode<int>> path = FindShortestPath(topology, graph, stopPoints);

                List<Troncon> troncons = TransformPathToEdgeList(topology, path);
                return troncons;
            }
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
            //TopoNode topoNodeEnd = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(stopPoints.Skip(1).First().Value.Geometry).IsTrue).Single().Value; // test with second node
            TopoNode topoNodeEnd = topology.TopoNodes.Where(n => n.Value.Geometry.STEquals(stopPoints.Last().Value.Geometry).IsTrue).Single().Value;

            GraphNode<int> start = graph.Where(n => n.Value == topoNodeStart.Id).Single();
            GraphNode<int> end = graph.Where(n => n.Value == topoNodeEnd.Id).Single();


            var dijkstra = new Dijkstra<int>(graph);
            var path = dijkstra.FindShortestPathBetween(start, end);

            return path;
        }

        private List<Troncon> TransformPathToEdgeList(Topology topology, IList<GraphNode<int>> path)
        {

            var listnodeIds = "(" + String.Join<int>("),(", path.Select(g => g.Value)) + ")";
            List<Troncon> pathInTroncons = new List<Troncon>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];

                var troncon = GetTronconBetweenNodes(topology, current.Value, next.Value);
                if (troncon != null)
                {
                    pathInTroncons.Add(GetTronconBetweenNodes(topology, current.Value, next.Value));
                }
            }
            return pathInTroncons;
        }

        private Troncon GetTronconBetweenNodes(Topology topology, int nodeIdFrom, int nodeIdTo)
        {
            var troncons = topology.TopoTroncons.Where(kvp => kvp.Value.IdNodes.Contains(nodeIdFrom) && kvp.Value.IdNodes.Contains(nodeIdTo))
                                 .Select(t => topology.Troncons.First(baseTrn => baseTrn.Id == t.Value.IdTroncon))
                                 .ToList();

            if (troncons.Count == 0)
            {
                Trace.TraceWarning($"No troncon found between nodes.");
                return null;
            }
            else if (troncons.Count > 1)
            {
                Trace.TraceWarning($"Ambiguous path between {string.Join<int>(", ", troncons.Select(t => t.Id))} edges. 1st will be chosen.");
                //Debug.Assert(troncons.Count == 1, "Ambiguous path. 1st will be chosen.");
                return troncons.First();
            }
            return troncons.Single();
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

        private IEnumerable<Troncon> FilterTronconsByCommercialMode(IEnumerable<Troncon> troncons, CommercialMode commercialMode)
        {
            if (commercialMode.Name != "TGV")
            {
                return troncons.Where(kvp => kvp.Nature != "LGV");
            }
            else { return troncons; }
        }


        #endregion
    }
}
