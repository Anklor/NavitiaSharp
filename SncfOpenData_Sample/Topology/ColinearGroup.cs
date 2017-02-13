using SncfOpenData.IGN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.Topology
{
    public class ColinearGroup
    {
        public double AngleDegrees { get; set; }
        public List<Troncon> ColinearTroncons { get; set; }

        public ColinearGroup(double angleDegrees, List<Troncon> colinearTroncons)
        {
            AngleDegrees = angleDegrees;
            ColinearTroncons = colinearTroncons;
        }
        public override string ToString()
        {
            return $"Edges {String.Join<int>(",", ColinearTroncons.Select(t => t.Id))} with angle {AngleDegrees} degrees.";
        }
    }
}
