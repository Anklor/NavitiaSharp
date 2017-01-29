using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{


    public class Calendar
    {
        [DeserializeAs(Name = "active_periods")]
        public List<ActivePeriod> ActivePeriods { get; set; }

        [DeserializeAs(Name = "week_pattern")]
        public WeekPattern WeekPattern { get; set; }
    }
}
