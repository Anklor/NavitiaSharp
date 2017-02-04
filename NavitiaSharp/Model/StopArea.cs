using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class StopArea : ApiResourceBase
    {

        [DeserializeAs(Name = "codes")]
        public List<Code> Codes { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "coord")]
        public Coord Coord { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        [DeserializeAs(Name = "administrative_regions")]
        public List<AdministrativeRegion> AdministrativeRegions { get; set; }

        [DeserializeAs(Name = "timezone")]
        public string Timezone { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Label})";
        }
    }
}
