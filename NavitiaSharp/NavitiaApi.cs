using NavitiaSharp;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class NavitiaApi
    {
        readonly string _apiKey;
        readonly string _baseUrl;

        public NavitiaApi(string baseUrl, string apiKey)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl;
        }

        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(_baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(_apiKey, null);

            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var twilioException = new ApplicationException(message, response.ErrorException);
                throw twilioException;
            }
            return response.Data;
        }

        public StopPointCollection GetStopPoint(string stopPointId)
        {
            var request = new RestRequest();
            request.Resource = "/stop_points/{stopPointId}";
            request.RootElement = "stop_points";

            request.AddParameter("stopPointId", stopPointId, ParameterType.UrlSegment);

            return Execute<StopPointCollection>(request);
        }
        public List<StopArea> GetStopArea(string stopAreaId)
        {
            var request = new RestRequest();
            request.Resource = "/stop_areas/{stopAreaId}";
            request.RootElement = "stop_areas";

            request.AddParameter("stopAreaId", stopAreaId, ParameterType.UrlSegment);

            return Execute<List<StopArea>>(request);
        }

    }

}
