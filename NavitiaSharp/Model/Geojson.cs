using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Geojson
    {

        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "coordinates")]
        public List<object> Coordinates { get; set; }
    }
}
