using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace SncfOpenData.IGN.Model
{
    public class Troncon
    {
        public string Classement { get; internal set; }
        public string Energie { get; internal set; }
        public SqlGeometry Geometry { get; internal set; }
        public int Id { get; internal set; }
        public string Nature { get; internal set; }
    }
}
