using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class JourneyPattern
    {

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }
    }


    public class Disruption
    {
        [DeserializeAs(Name = "internal")]
        public bool Internal { get; set; }

        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "rel")]
        public string Rel { get; set; }

        [DeserializeAs(Name = "templated")]
        public bool Templated { get; set; }

        [DeserializeAs(Name = "status")]
        public string Status { get; set; }

        [DeserializeAs(Name = "disruption_id")]
        public string DisruptionId { get; set; }

        [DeserializeAs(Name = "severity")]
        public Severity Severity { get; set; }

        [DeserializeAs(Name = "impact_id")]
        public string ImpactId { get; set; }

        [DeserializeAs(Name = "application_periods")]
        public List<ApplicationPeriod> ApplicationPeriods { get; set; }

        [DeserializeAs(Name = "updated_at")]
        public string UpdatedAt { get; set; }

        [DeserializeAs(Name = "uri")]
        public string Uri { get; set; }

        [DeserializeAs(Name = "impacted_objects")]
        public List<ImpactedObject> ImpactedObjects { get; set; }

        [DeserializeAs(Name = "disruption_uri")]
        public string DisruptionUri { get; set; }

        [DeserializeAs(Name = "contributor")]
        public string Contributor { get; set; }

        [DeserializeAs(Name = "cause")]
        public string Cause { get; set; }

    }

    public class ActivePeriod
    {

        [DeserializeAs(Name = "begin")]
        public string Begin { get; set; }

        [DeserializeAs(Name = "end")]
        public string End { get; set; }
    }

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

    public class Calendar
    {

        [DeserializeAs(Name = "active_periods")]
        public List<ActivePeriod> ActivePeriods { get; set; }

        [DeserializeAs(Name = "week_pattern")]
        public WeekPattern WeekPattern { get; set; }
    }



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

    public class ValidityPattern
    {

        [DeserializeAs(Name = "beginning_date")]
        public string BeginningDate { get; set; }

        [DeserializeAs(Name = "days")]
        public string Days { get; set; }
    }

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

    public class Severity
    {

        [DeserializeAs(Name = "color")]
        public string Color { get; set; }

        [DeserializeAs(Name = "priority")]
        public int Priority { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }

        [DeserializeAs(Name = "effect")]
        public string Effect { get; set; }
    }

    public class ApplicationPeriod
    {

        [DeserializeAs(Name = "begin")]
        public string Begin { get; set; }

        [DeserializeAs(Name = "end")]
        public string End { get; set; }
    }


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

    public class Trip
    {

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }
    }


    public class PtObject
    {

        [DeserializeAs(Name = "embedded_type")]
        public string EmbeddedType { get; set; }

        [DeserializeAs(Name = "trip")]
        public Trip Trip { get; set; }

        [DeserializeAs(Name = "quality")]
        public int Quality { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }
    }

    public class ImpactedObject
    {

        [DeserializeAs(Name = "impacted_stops")]
        public List<ImpactedStop> ImpactedStops { get; set; }

        [DeserializeAs(Name = "pt_object")]
        public PtObject PtObject { get; set; }
    }



}
