using NavitiaSharp;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        #region EXecute wrappers

        public async Task<T> ExecuteTaskAsync<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(_baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(_apiKey, null);

            var response = await client.ExecuteGetTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var exception = new ApplicationException(message, response.ErrorException);
                throw exception;
            }
            return response.Data;
        }
        public T Execute<T>(RestRequest request, string resourcePath = null) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(_baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(_apiKey, null);
            if (resourcePath.Contains("/"))
            {
                resourcePath = resourcePath.Split('/').Last();
            }
            client.AddHandler("application/json", new NavitiaSharp.Deserializers.JsonDeserializer(resourcePath));

            request.RequestFormat = DataFormat.Json;
            request.DateFormat = "yyyyMMddTHHmmss";
            
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var exception = new ApplicationException(message, response.ErrorException);
                throw exception;
            }
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return response.Data;
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new  UnauthorizedAccessException("Unauthorized. Check you have provided a valid API key.");
                case System.Net.HttpStatusCode.NotFound:
                    throw new KeyNotFoundException("NotFound.");
                default:
                    string message = $"Error retrieving response : {response.StatusDescription}";
                    var exception = new ApplicationException(message);
                    throw exception;
            }
        }

        #endregion

        public List<StopPoint> GetStopPoints()
        {
            var request = new RestRequest();
            request.Resource = "/stop_points";
            request.RootElement = "stop_points";

            return Execute<List<StopPoint>>(request);
        }
        public StopPoint GetStopPoint(string stopPointId)
        {
            var request = new RestRequest();
            request.Resource = "/stop_points/{stopPointId}";
            request.RootElement = "stop_points";

            request.AddParameter("stopPointId", stopPointId, ParameterType.UrlSegment);

            return Execute<List<StopPoint>>(request).FirstOrDefault();
        }
        public StopArea GetStopArea(string stopAreaId)
        {
            var request = new RestRequest();
            request.Resource = "/stop_areas/{stopAreaId}";
            request.RootElement = "stop_areas";

            request.AddParameter("stopAreaId", stopAreaId, ParameterType.UrlSegment);

            return Execute<List<StopArea>>(request).FirstOrDefault();
        }
        public Line GetLine(string lineId)
        {
            var request = new RestRequest();
            request.Resource = "/lines/{lineId}";
            request.RootElement = "lines";

            request.AddParameter("lineId", lineId, ParameterType.UrlSegment);

            return Execute<List<Line>>(request).FirstOrDefault();
        }
        public PagedResult<RouteSchedule> GetLineRouteSchedules(string lineId, int numResults = 25, int numPage = 0)
        {
            var request = new RestRequest();
            request.Resource = $"/lines/{lineId}/route_schedules";

            return GetPagedResult<RouteSchedule>(request, "route_schedules", numResults, numPage);      
        }

        public PagedResult<T> GetPagedResult<T>(string resourcePath, int numResults = 25, int numPage = 0) where T : new()
        {
            var request = new RestRequest();
            request.Resource = "/" + resourcePath + "/";

            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<PagedResult<T>>(request, resourcePath);
        }
        public PagedResult<T> GetPagedResult<T>(RestRequest request, string resourcePath, int numResults = 25, int numPage = 0) where T : new()
        {
            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<PagedResult<T>>(request, resourcePath);
        }
        public List<T> GetListResult<T>(string resourcePath, string rootElement, int numResults = 25, int numPage = 0) where T : new()
        {
            var request = new RestRequest();
            request.Resource = "/" + resourcePath + "/";
            request.RootElement = rootElement;


            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<List<T>>(request);
        }

    }

}
