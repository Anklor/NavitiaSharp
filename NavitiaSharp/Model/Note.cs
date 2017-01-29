using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{

    public class Note
    {
        [DeserializeAs(Name = "type")]
        public string Type { get; set; }

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "value")]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
