using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData.Model
{
    public class StopAreaIGN
    {
        public string StopAreaId { get; set; }

        public bool HasIGNMatch { get { return IdNoeud > 0; } }
        public int IdNoeud { get; set; }
        public string NomNoeud { get; set; }
        public double DistanceNoeud { get; set; }
    }
}
