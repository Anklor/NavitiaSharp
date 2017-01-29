using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class WeekPattern
    {

        [DeserializeAs(Name = "monday")]
        public bool Monday { get; set; }

        [DeserializeAs(Name = "tuesday")]
        public bool Tuesday { get; set; }

        [DeserializeAs(Name = "friday")]
        public bool Friday { get; set; }

        [DeserializeAs(Name = "wednesday")]
        public bool Wednesday { get; set; }

        [DeserializeAs(Name = "thursday")]
        public bool Thursday { get; set; }

        [DeserializeAs(Name = "sunday")]
        public bool Sunday { get; set; }

        [DeserializeAs(Name = "saturday")]
        public bool Saturday { get; set; }
    }

}
