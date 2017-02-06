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
        public Dictionary<int, HashSet<int>> NodesByTroncon { get; private set; }
        public Dictionary<int, HashSet<int>> TronconsByNodes { get; private set; }
        public Dictionary<int, Troncon> Troncons { get; private set; }
        public Dictionary<int, Noeud> Nodes { get; private set; }

        public static Topology Compute(List<Troncon> tronconsInRoute, List<Noeud> nodesInRoute)
        {
            var topology = new Topology();
            topology.Troncons = tronconsInRoute.ToDictionary(t => t.Id, t => t);
            topology.Nodes = nodesInRoute.ToDictionary(n => n.Id, n => n); 
            topology.Compute();

            return topology;
        }

        private void Compute()
        {
            NodesByTroncon = GetTopologyByTroncon(Troncons, Nodes);
            TronconsByNodes = GetTopologyByNode(Troncons, Nodes);
        }

        private Dictionary<int, HashSet<int>> GetTopologyByTroncon(Dictionary<int, Troncon> tronconsInRoute, Dictionary<int, Noeud> nodesInRoute)
        {
            var query = from troncon in tronconsInRoute
                        let connectedNodes = nodesInRoute.Where(n => n.Value.Geometry.STIntersects(troncon.Value.Geometry).IsTrue)
                        select new { IdTroncon = troncon.Key, Nodes = connectedNodes.Select(n => n.Key) };
            return query.ToDictionary(a => a.IdTroncon, a => new HashSet<int>(a.Nodes));
        }
        private Dictionary<int, HashSet<int>> GetTopologyByNode(Dictionary<int, Troncon> tronconsInRoute, Dictionary<int, Noeud> nodesInRoute)
        {
            var query = from node in nodesInRoute
                        let connectedTroncons = tronconsInRoute.Where(t => t.Value.Geometry.STIntersects(node.Value.Geometry).IsTrue)
                        select new { IdNode = node.Key, Troncons = connectedTroncons.Select(t => t.Key) };
            return query.ToDictionary(a => a.IdNode, a => new HashSet<int>(a.Troncons));
        }
    }
}
