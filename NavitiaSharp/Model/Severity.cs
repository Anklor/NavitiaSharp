using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

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

}
