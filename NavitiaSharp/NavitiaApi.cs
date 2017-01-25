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
        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(_baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(_apiKey, null);

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
                    throw new Exception("Unauthorized. Check you have provided a valid API key.");
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
        public List<StopArea> GetStopAreas(int numResults = 25, int numPage = 0)
        {
            var request = new RestRequest();
            request.Resource = "/stop_areas/";
            request.RootElement = "stop_areas";


            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<List<StopArea>>(request);
        }
        public PagedResult<StopArea> GetStopAreasPaged(int numResults = 25, int numPage = 0)
        {
            var request = new RestRequest();
            request.Resource = "/stop_areas/";


            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<PagedResult<StopArea>>(request);
        }
        public PagedResult<T> GetPagedResult<T>(string resourcePath, int numResults = 25, int numPage = 0) where T : new()
        {
            var request = new RestRequest();
            request.Resource = "/" + resourcePath  +"/";


            request.AddParameter("count", numResults, ParameterType.QueryString);
            request.AddParameter("start_page", numPage, ParameterType.QueryString);

            return Execute<PagedResult<T>>(request);
        }

    }

}
