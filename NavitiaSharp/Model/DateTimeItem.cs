using RestSharp.Deserializers;
using System;
using System.Collections.Generic;

namespace NavitiaSharp
{

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

        public override string ToString()
        {
            return DateTime.ToString();
        }
    }

}
