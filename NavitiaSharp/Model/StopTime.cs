using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class StopTime
    {

        [DeserializeAs(Name = "arrival_time")]
        public string ArrivalTime { get; set; }

        [DeserializeAs(Name = "headsign")]
        public string Headsign { get; set; }

        [DeserializeAs(Name = "departure_time")]
        public string DepartureTime { get; set; }

        [DeserializeAs(Name = "stop_point")]
        public StopPoint StopPoint { get; set; }
    }


}
