using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Coord
    {

        [DeserializeAs(Name = "lat")]
        public string Lat { get; set; }

        [DeserializeAs(Name = "lon")]
        public string Lon { get; set; }
    }
}
