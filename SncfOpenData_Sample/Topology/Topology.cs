using Microsoft.SqlServer.Types;
using SncfOpenData.App.Topology;
using SncfOpenData.IGN.Model;
using SqlServerSpatial.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class Topology
    {
        public List<Troncon> Troncons { get; private set; }
        public List<Noeud> Nodes { get; private set; }

        public Dictionary<int, TopoNode> TopoNodes = new Dictionary<int, TopoNode>();
        public Dictionary<int, TopoTroncon> TopoTroncons = new Dictionary<int, TopoTroncon>();

        private Topology(List<Troncon> troncons, List<Noeud> nodes)
        {
            Troncons = troncons;
            Nodes = nodes;
        }

        public static Topology Compute(List<Troncon> troncons, List<Noeud> nodes)
        {
            var topology = new Topology(troncons, nodes);
            topology.Compute();

            return topology;
        }

        private void Compute()
        {
            // 1. Add all edges start and end points -> distinct topology nodes

            // We want to have a pointer for each troncon to start and end geometry
            int index = 0;
            foreach (Troncon troncon in Troncons)
            {
                SqlGeometry start = troncon.StartPoint;
                SqlGeometry end = troncon.EndPoint;

                if (!NodeAdded(start))
                {
                    AddNode(start, index++);
                }
                if (!NodeAdded(end))
                {
                    AddNode(end, index++);
                }

            }

            // 2. get connections between nodes and edges

            Dictionary<int, TopoNode> newNodes = new Dictionary<int, TopoNode>();
            foreach (var topoNode in TopoNodes)
            {
                var connectedTroncons = Troncons.Where(t => t.StartPoint.STEquals(topoNode.Value.Geometry).IsTrue
                                                            || t.EndPoint.STEquals(topoNode.Value.Geometry).IsTrue)
                                                            .ToList();
                // Is the topology node an "IGN node" ?
                // Yes => proceed
                // No => IGN indicates that this is a bridge or tunnel and there are in fact
                // multiple nodes connecting only two edges at once
                if (IsIgnNode(topoNode) || connectedTroncons.Count == 1)
                {
                    topoNode.Value.IdTroncons.UnionWith(connectedTroncons.Select(t => t.Id));
                }
                else
                {
                    Debug.Assert(connectedTroncons.Count % 2 == 0, "Bridge or tunnel has a weird number of connections");

                    List<List<Troncon>> colinearGroups = FindColinearTroncons(connectedTroncons, topoNode.Value.Geometry);

                    bool firstGroup = true;
                    foreach (var group in colinearGroups)
                    {
                        // avoid to make topoNode orphan
                        // add new node on second iteration only
                        if (!firstGroup)
                        {
                            index++;
                            var newNode = new TopoNode(index, topoNode.Value);
                            newNodes.Add(index, newNode);
                            newNode.IdTroncons.UnionWith(group.Select(t => t.Id));
                        }
                        else
                        {
                            Debug.Assert(topoNode.Value.IdTroncons.Count == 0, "Toponode is already connected.");
                            topoNode.Value.IdTroncons.UnionWith(group.Select(t => t.Id));
                            firstGroup = false;
                        }
                    }
                }
            }

            // 3. Merge nodes
            foreach (var newNode in newNodes)
            {
                TopoNodes.Add(newNode.Key, newNode.Value);
            }

            TopoTroncons = new Dictionary<int, TopoTroncon>();
            foreach (var topoNode in TopoNodes)
            {
                foreach (var idTroncon in topoNode.Value.IdTroncons)
                {
                    if (!TopoTroncons.ContainsKey(idTroncon))
                    {
                        TopoTroncons[idTroncon] = new TopoTroncon { IdTroncon = idTroncon, Geometry = Troncons.First(t => t.Id == idTroncon).Geometry };
                    }
                    TopoTroncons[idTroncon].IdNodes.Add(topoNode.Value.Id);
                }
            }

        }

        private List<List<Troncon>> FindColinearTroncons(List<Troncon> connectedTroncons, SqlGeometry intersectionPoint)
        {
            HashSet<int> visitedIds = new HashSet<int>();
            List<List<Troncon>> groups = new List<List<Troncon>>();
            foreach (var troncon in connectedTroncons)
            {
                if (visitedIds.Contains(troncon.Id))
                    continue;

                Troncon colinear = connectedTroncons.Where(t => t.Id != troncon.Id)
                                    .FirstOrDefault(t => !visitedIds.Contains(t.Id) && Geometry.AreColinear(troncon.Geometry, t.Geometry, intersectionPoint));
                //Debug.Assert(colinear != null, "Cannot find colinear troncon");

                if (colinear != null)
                {
                    visitedIds.Add(troncon.Id);
                    visitedIds.Add(colinear.Id);

                    groups.Add(new List<Troncon> { troncon, colinear });
                }
            }
            return groups;
        }


        private bool IsIgnNode(KeyValuePair<int, TopoNode> topoNode)
        {
            bool eq = Nodes.Any(n => n.Geometry.STEquals(topoNode.Value.Geometry).IsTrue);
            return eq;
        }

        private void AddNode(SqlGeometry newNode, int nodeIndex)
        {
            TopoNode newTopoNode = new TopoNode { Id = nodeIndex, Geometry = newNode };
            TopoNodes.Add(newTopoNode.Id, newTopoNode);
        }

        private bool NodeAdded(SqlGeometry nodeGeom)
        {
            return TopoNodes.Values.Any(n => n.Geometry.STEquals(nodeGeom).IsTrue);
        }

        private void SpatialTrace_Connexions(List<Troncon> connectedTroncons, KeyValuePair<int, TopoNode> topoNode)
        {
            SpatialTrace.Enable();
            SpatialTrace.Indent(connectedTroncons.Count.ToString() + " connections - node " + topoNode.Key.ToString());
            foreach (var trn in connectedTroncons)
            {
                SpatialTrace.TraceGeometry(trn.Geometry, trn.Id.ToString(), trn.Id.ToString());
            }

            foreach (var troncon in connectedTroncons)
            {
                foreach (var tronconOther in connectedTroncons.Where(t => t.Id != troncon.Id))
                {
                    var angle = Geometry.AngleBetweenLines(troncon.Geometry, tronconOther.Geometry);
                    SqlGeometry connectionPoint = troncon.Geometry.STIntersection(tronconOther.Geometry);

                    SqlGeometry segment1 = Geometry.FirstSegmentFrom(troncon.Geometry, connectionPoint);
                    SqlGeometry segment2 = Geometry.FirstSegmentFrom(tronconOther.Geometry, connectionPoint);
                    SpatialTrace.TraceGeometry(segment1.STUnion(segment2), $"{troncon.Id}<->{tronconOther.Id} - angle: {angle * 180 / Math.PI}", $"angle: {angle}");
                }
            }
            SpatialTrace.TraceGeometry(topoNode.Value.Geometry.STBuffer(50), "node " + topoNode.Value.Id.ToString(), "node " + topoNode.Value.Id.ToString());
            SpatialTrace.Unindent();
            SpatialTrace.Disable();
        }
    }
}
