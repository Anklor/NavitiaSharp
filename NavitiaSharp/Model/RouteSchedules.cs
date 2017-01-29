using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{
    public class RouteSchedule
    {
        [DeserializeAs(Name = "display_informations")]
        public DisplayInformations DisplayInformations { get; set; }

        [DeserializeAs(Name = "table")]
        public Table Table { get; set; }

        [DeserializeAs(Name = "additional_informations")]
        public string AdditionalInformations { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "geojson")]
        public Geojson Geojson { get; set; }

        public override string ToString()
        {
            return $"{DisplayInformations}";
        }
    }

}
