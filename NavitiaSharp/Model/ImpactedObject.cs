using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class ImpactedObject
    {

        [DeserializeAs(Name = "impacted_stops")]
        public List<ImpactedStop> ImpactedStops { get; set; }

        [DeserializeAs(Name = "pt_object")]
        public PtObject PtObject { get; set; }
    }

}
