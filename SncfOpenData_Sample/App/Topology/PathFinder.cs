using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SncfOpenData.IGN.Model;
using Microsoft.SqlServer.Types;
using SqlServerSpatial.Toolkit;

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
            SqlGeometry geom = GetNodesGeometryAggregate(FilterNodesById(_nodes, checkpoints));
            geom = geom.STEnvelope().STBuffer(bufferAroundPoints).STEnvelope();

            var tronconsInRoute = FilterTronconsByGeometry(_troncons, geom).ToList();
            var nodesInRoute = FilterNodesByGeometry(_nodes, geom).ToList();

            // Generate topology
            var topology = Topology.Compute(tronconsInRoute, nodesInRoute);
        }

        private SqlGeometry GetNodesGeometryAggregate(IEnumerable<Noeud> nodes)
        {
            SqlGeometry geom = SqlTypesExtensions.PointEmpty_SqlGeometry(2154);
            foreach (var pointGeom in nodes)
            {
                geom = geom.STUnion(pointGeom.Geometry);
            }
            return geom;
        }

        private IEnumerable<Noeud> FilterNodesById(Dictionary<int, Noeud> nodes, HashSet<int> keys)
        {
            return nodes.Where(kvp => keys.Contains(kvp.Key)).Select(kvp => kvp.Value);
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
    }
}
