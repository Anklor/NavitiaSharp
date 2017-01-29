using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavitiaSharp
{

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
}
