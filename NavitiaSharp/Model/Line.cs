using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{
    public class Line
    {
        [DeserializeAs(Name = "code")]
        public string Code { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "physical_modes")]
        public List<PhysicalMode> PhysicalModes { get; set; }

        [DeserializeAs(Name = "opening_time")]
        public string OpeningTime { get; set; }

        [DeserializeAs(Name = "geojson")]
        public Geojson Geojson { get; set; }

        [DeserializeAs(Name = "text_color")]
        public string TextColor { get; set; }

        [DeserializeAs(Name = "color")]
        public string Color { get; set; }

        [DeserializeAs(Name = "closing_time")]
        public string ClosingTime { get; set; }

        [DeserializeAs(Name = "routes")]
        public List<Route> Routes { get; set; }

        [DeserializeAs(Name = "commercial_mode")]
        public CommercialMode CommercialMode { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "network")]
        public Network Network { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }


}