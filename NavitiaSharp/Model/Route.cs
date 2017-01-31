using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Route : IApiResource
    {

        [DeserializeAs(Name = "direction")]
        public Direction Direction { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "is_frequence")]
        public string IsFrequence { get; set; }

        [DeserializeAs(Name = "geojson")]
        public Geojson Geojson { get; set; }

        [DeserializeAs(Name = "direction_type")]
        public string DirectionType { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "line")]
        public Line Line { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
