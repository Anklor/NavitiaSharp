using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class ValidityPattern
    {

        [DeserializeAs(Name = "beginning_date")]
        public string BeginningDate { get; set; }

        [DeserializeAs(Name = "days")]
        public string Days { get; set; }
    }
}
