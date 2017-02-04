using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{
    public abstract class ApiResourceBase : IEquatable<ApiResourceBase>
    {
        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        public bool Equals(ApiResourceBase other)
        {
            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public bool Equals(Line other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is ApiResourceBase)
            {
                return Equals((ApiResourceBase)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
