using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class DisplayInformations
    {
        [DeserializeAs(Name = "direction")]
        public string Direction { get; set; }

        [DeserializeAs(Name = "code")]
        public string Code { get; set; }

        [DeserializeAs(Name = "description")]
        public string Description { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "color")]
        public string Color { get; set; }

        [DeserializeAs(Name = "physical_mode")]
        public string PhysicalMode { get; set; }

        [DeserializeAs(Name = "headsign")]
        public string Headsign { get; set; }

        [DeserializeAs(Name = "commercial_mode")]
        public string CommercialMode { get; set; }

        [DeserializeAs(Name = "equipments")]
        public List<object> Equipments { get; set; }

        [DeserializeAs(Name = "text_color")]
        public string TextColor { get; set; }

        [DeserializeAs(Name = "network")]
        public string Network { get; set; }

        [DeserializeAs(Name = "label")]
        public string Label { get; set; }

        public override string ToString()
        {
            return $"{Label}";
        }
    }

}
