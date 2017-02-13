using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;

namespace SncfOpenData.IGN.Model
{
    public class Troncon : IEquatable<Troncon>
    {
        public string Classement { get; internal set; }
        public string Energie { get; internal set; }
        private SqlGeometry _geometry;
        public SqlGeometry StartPoint { get; private set; }
        public SqlGeometry EndPoint { get; private set; }
        
        public SqlGeometry Geometry
        {
            get { return _geometry; }
            set
            {
                _geometry = value;
                StartPoint = _geometry.STStartPoint();
                EndPoint = _geometry.STEndPoint();
            }
        }
        public int Id { get; internal set; }
        public string Nature { get; internal set; }

        public bool Equals(Troncon other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Troncon)
            {
                return Equals((Troncon)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Id} - {Nature}";
        }
    }
}

