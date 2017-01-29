using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class VehicleJourney
    {

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "journey_pattern")]
        public JourneyPattern JourneyPattern { get; set; }

        [DeserializeAs(Name = "disruptions")]
        public List<Disruption> Disruptions { get; set; }

        [DeserializeAs(Name = "calendars")]
        public List<Calendar> Calendars { get; set; }

        [DeserializeAs(Name = "stop_times")]
        public List<StopTime> StopTimes { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "validity_pattern")]
        public ValidityPattern ValidityPattern { get; set; }

        [DeserializeAs(Name = "trip")]
        public Trip Trip { get; set; }
    }

}
