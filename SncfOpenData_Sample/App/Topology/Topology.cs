using Microsoft.SqlServer.Types;
using SncfOpenData.App.Topology;
using SncfOpenData.IGN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class Topology
    {
        public List<Troncon> Troncons { get; private set; }
        public List<Noeud> Nodes { get; private set; }

        public List<TopoNode> TopoNodes = new List<TopoNode>();

        public static Topology Compute(List<Troncon> tronconsInRoute, List<Noeud> nodesInRoute)
        {
            var topology = new Topology();
            topology.Troncons = tronconsInRoute;
            topology.Nodes = nodesInRoute;
            topology.Compute();

            return topology;
        }

        private void Compute()
        {
            // We want to have a pointer for each troncon to start and end geometry

            int index = 0;
            foreach (Troncon troncon in Troncons)
            {
                SqlGeometry start = troncon.Geometry.STStartPoint();
                SqlGeometry end = troncon.Geometry.STEndPoint();

                if (!TopoNodes.Any(n => n.Geometry.STEquals(start).IsTrue))
                {
                    TopoNode startTNode = new TopoNode { Id = index++, IsStart = true, Geometry = start };
                    TopoNodes.Add(startTNode);
                }
                if (!TopoNodes.Any(n => n.Geometry.STEquals(end).IsTrue))
                {
                    TopoNode endTNode = new TopoNode { Id = index++, IsStart = false, Geometry = end };
                    TopoNodes.Add(endTNode);
                }
            }

            foreach (var topoNode in TopoNodes)
            {
                var connectedTroncons = Troncons.Where(t => t.Geometry.STStartPoint().STEquals(topoNode.Geometry).IsTrue
                                                                    || t.Geometry.STEndPoint().STEquals(topoNode.Geometry).IsTrue)
                                                                    .ToList();

                topoNode.IdTroncons.UnionWith(connectedTroncons.Select(t => t.Id));
            }


        }




    }
}
