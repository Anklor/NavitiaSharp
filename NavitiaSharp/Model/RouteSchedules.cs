using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{

    public class Note
    {
        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "value")]
        public string Value { get; set; }
    }
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
    }



    public class Header
    {

        [DeserializeAs(Name = "display_informations")]
        public DisplayInformations DisplayInformations { get; set; }

        [DeserializeAs(Name = "additional_informations")]
        public List<string> AdditionalInformations { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }
    }

    public class DateTimeItem
    {

        [DeserializeAs(Name = "date_time")]
        public DateTime DateTime { get; set; }

        [DeserializeAs(Name = "additional_informations")]
        public List<object> AdditionalInformations { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "data_freshness")]
        public string DataFreshness { get; set; }
    }

    public class Row
    {

        [DeserializeAs(Name = "stop_point")]
        public StopPoint StopPoint { get; set; }

        [DeserializeAs(Name = "date_times")]
        public List<DateTimeItem> DateTimes { get; set; }
    }

    public class Table
    {

        [DeserializeAs(Name = "headers")]
        public List<Header> Headers { get; set; }

        [DeserializeAs(Name = "rows")]
        public List<Row> Rows { get; set; }
    }



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
    }





}
