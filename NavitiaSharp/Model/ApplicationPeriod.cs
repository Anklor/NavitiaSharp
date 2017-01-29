using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class ApplicationPeriod
    {

        [DeserializeAs(Name = "begin")]
        public string Begin { get; set; }

        [DeserializeAs(Name = "end")]
        public string End { get; set; }

        public override string ToString()
        {
            return $"{Begin} to {End}";
        }
    }

}
