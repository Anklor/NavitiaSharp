using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{
    public class Code
    {

        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "value")]
        public string Value { get; set; }
    }

    public class Coord
    {

        [DeserializeAs(Name = "lat")]
        public string Lat { get; set; }

        [DeserializeAs(Name = "lon")]
        public string Lon { get; set; }
    }

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


    public class StopArea
    {

        [DeserializeAs(Name = "codes")]
        public List<Code> Codes { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<object> Links { get; set; }

        [DeserializeAs(Name = "coord")]
        public Coord Coord { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        [DeserializeAs(Name = "administrative_regions")]
        public List<AdministrativeRegion> AdministrativeRegions { get; set; }

        [DeserializeAs(Name = "timezone")]
        public string Timezone { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }
    }

    public class StopPoint
    {

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<object> Links { get; set; }

        [DeserializeAs(Name = "coord")]
        public Coord Coord { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        [DeserializeAs(Name = "equipments")]
        public List<object> Equipments { get; set; }

        [DeserializeAs(Name = "administrative_regions")]
        public List<AdministrativeRegion> AdministrativeRegions { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "stop_area")]
        public StopArea StopArea { get; set; }
    }

    public class StopPointCollection
    {

        [DeserializeAs(Name = "stop_points")]
        public List<StopPoint> StopPoints { get; set; }
    }
}
