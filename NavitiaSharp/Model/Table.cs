using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            string ret = $"{StopPoint}";
            if (DateTimes != null && DateTimes.Any())
            {
                ret = string.Concat(ret, " : ", string.Join<DateTimeItem>(", ", DateTimes));
            }
            return ret;
        }
    }

    public class Table
    {

        [DeserializeAs(Name = "headers")]
        public List<Header> Headers { get; set; }

        [DeserializeAs(Name = "rows")]
        public List<Row> Rows { get; set; }

        public bool WithSchedule
        {
            get
            {
                return Rows.Any(r => r.DateTimes.Any(dt => dt.DateTime != default(DateTime)));
            }
        }

        public override string ToString()
        {
            string ret =  $"{Rows.Count} row(s)";
            if (WithSchedule)
            { 
                ret += " with schedules.";
            }
            return ret;
        }
    }


}
