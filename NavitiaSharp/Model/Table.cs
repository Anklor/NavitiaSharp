using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Header
    {

        [DeserializeAs(Name = "display_informations")]
        public DisplayInformations DisplayInformations { get; set; }

        [DeserializeAs(Name = "additional_informations")]
        public List<string> AdditionalInformations { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }
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


}
