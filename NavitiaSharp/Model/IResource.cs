using RestSharp.Deserializers;

namespace NavitiaSharp
{
    public interface IApiResource
    {
        [DeserializeAs(Name = "id")]
        string Id { get; set; }
    }
}