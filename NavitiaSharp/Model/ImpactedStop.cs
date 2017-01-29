using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class ImpactedStop
    {

        [DeserializeAs(Name = "amended_arrival_time")]
        public string AmendedArrivalTime { get; set; }

        [DeserializeAs(Name = "stop_point")]
        public StopPoint StopPoint { get; set; }

        [DeserializeAs(Name = "stop_time_effect")]
        public string StopTimeEffect { get; set; }

        [DeserializeAs(Name = "amended_departure_time")]
        public string AmendedDepartureTime { get; set; }

        [DeserializeAs(Name = "base_arrival_time")]
        public string BaseArrivalTime { get; set; }

        [DeserializeAs(Name = "cause")]
        public string Cause { get; set; }

        [DeserializeAs(Name = "base_departure_time")]
        public string BaseDepartureTime { get; set; }
    }
}
