using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using System.Diagnostics;

namespace SncfOpenData.Topology
{
	public class TopoNode : IEquatable<TopoNode>
	{
		public int Id { get; set; }
		public HashSet<int> IdTroncons { get; set; }
		public SqlGeometry Geometry { get; set; }

        /// <summary>
        /// When node is a "virtual" node built to handle line crossings, this field
        /// contains the original node Id (useful to reconstruct path)
        /// </summary>
        //public int OriginalNodeId { get; private set; }

		public TopoNode()
		{
			IdTroncons = new HashSet<int>();
		}
        public TopoNode(int newId, TopoNode original) : this()
        {
            Id = newId;
            //Debug.Assert(original.OriginalNodeId == 0, "Original node is a virtual node.");
            //OriginalNodeId = original.Id;
            Geometry = original.Geometry;
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
