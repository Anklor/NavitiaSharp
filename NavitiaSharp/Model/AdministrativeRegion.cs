using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class AdministrativeRegion
    {

        [DeserializeAs(Name = "insee")]
        public string Insee { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "level")]
        public int Level { get; set; }

        [DeserializeAs(Name = "coord")]
        public Coord Coord { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "zip_code")]
        public string ZipCode { get; set; }
    }
}
