using RestSharp.Deserializers;
using System.Collections.Generic;

namespace NavitiaSharp
{
    public class Trip
    {

        [DeserializeAs(Name = "id")]
        public string Id { get; set; }

        [DeserializeAs(Name = "name")]
        public string Name { get; set; }
    }

}
