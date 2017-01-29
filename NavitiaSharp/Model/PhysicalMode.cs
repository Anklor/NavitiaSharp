using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class PhysicalMode
    {

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
