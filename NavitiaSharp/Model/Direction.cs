using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Direction
    {

        [DeserializeAs(Name = "embedded_type")]
        public string EmbeddedType { get; set; }

        [DeserializeAs(Name = "quality")]
        public int Quality { get; set; }

        [DeserializeAs(Name = "stop_area")]
        public StopArea StopArea { get; set; }

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
