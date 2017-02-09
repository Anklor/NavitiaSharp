using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace SncfOpenData.App.Topology
{
	public class TopoNode : IEquatable<TopoNode>
	{
		public int Id { get; set; }
		public HashSet<int> IdTroncons { get; set; }
		public SqlGeometry Geometry { get; set; }

		public TopoNode()
		{
			IdTroncons = new HashSet<int>();
		}
		public bool Equals(TopoNode other)
		{
			return this.GetHashCode().Equals(other.GetHashCode());
		}

		public override int GetHashCode()
		{
			//int hash = IsStart.GetHashCode() * 17 + IdTroncon.GetHashCode();
			//return hash;
			return Id;
		}

		public override bool Equals(object obj)
		{
			TopoNode objTyped = obj as TopoNode;
			if (objTyped == null)
				return false;

			return this.Equals(objTyped);
		}

        public override string ToString()
        {
            return Id.ToString();
        }
    }

    public class TopoTroncon : IEquatable<TopoTroncon>
    {
        public int IdTroncon { get; set; }
        public HashSet<int> IdNodes { get; set; }
        public SqlGeometry Geometry { get; set; }

        public TopoTroncon()
        {
            IdNodes = new HashSet<int>();
        }
        public bool Equals(TopoTroncon other)
        {
            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            return IdTroncon;
        }

        public override bool Equals(object obj)
        {
            TopoTroncon objTyped = obj as TopoTroncon;
            if (objTyped == null)
                return false;

            return this.Equals(objTyped);
        }
    }
}
