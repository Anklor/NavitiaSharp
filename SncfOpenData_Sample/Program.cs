
using NavitiaSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    class Program
    {
        const int DEFAULT_CHUNK_SIZE = 1000;
        static void Main(string[] args)
        {
            string sncfAuthKey = ConfigurationManager.AppSettings["SNCF_API_KEY"];
            SncfApi sncfApi = new SncfApi(sncfAuthKey);


            //SaveStaticData(sncfApi);

            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            List<Line> lines = LoadSavedData<Line>("lines.json");
            List<Route> routes = LoadSavedData<Route>("routes.json");
            List<StopArea> stopAreas = LoadSavedData<StopArea>("stop_areas.json");
            List<StopPoint> stopPoints = LoadSavedData<StopPoint>("stop_points.json");

            string str2Find = "MEYRARGUES";
            TestQueryWithStopName(lines, routes, stopAreas, stopPoints, str2Find);
            str2Find = "AIX-EN-PROVENCE";
            TestQueryWithStopName(lines, routes, stopAreas, stopPoints, str2Find);

            string idToFind = "line:OCE:343";
            TestQueryWithId(lines, routes, stopAreas, stopPoints, idToFind);
          
            //var saQuery = stopAreas.Where(sa => sa.Name.ToUpper().Contains("AIX-EN-PROVENCE")).ToList();

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            //SaveStaticData(sncfApi);

            //var sa = sncfApi.GetStopArea("stop_area:OCE:SA:87113001");
            //StopPoint sp = sncfApi.GetStopPoint("stop_point:OCE:SP:CorailIntercité-87113001");


        }

        private static void TestQueryWithStopName(List<Line> lines, List<Route> routes, List<StopArea> stopAreas, List<StopPoint> stopPoints, string str2Find)
        {
            var saQuery = stopAreas.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var spQuery = stopPoints.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var linesQuery = lines.Where(obj => obj.Routes != null && obj.Routes.Any(r => r.Direction.Name.ToUpper().Contains(str2Find))).ToList();
            var routesQuery = routes.Where(obj => obj.Direction.Name.ToUpper().Contains(str2Find)).ToList();
        }
        private static void TestQueryWithId(List<Line> lines, List<Route> routes, List<StopArea> stopAreas, List<StopPoint> stopPoints, string idToFind)
        {
            var saQuery = stopAreas.Where(obj => obj.Id == idToFind).ToList();
            var linesQuery = lines.Where(obj => obj.Id == idToFind).ToList();
            var routesQuery = routes.Where(obj => obj.Id == idToFind).ToList();
            var spQuery = stopPoints.Where(obj => obj.Id == idToFind).ToList();
        }

        private static void SaveStaticData(SncfApi sncfApi)
        {
            GetAndSaveData<Line>(sncfApi, "lines", "lines.json");
            GetAndSaveData<StopArea>(sncfApi, "stop_areas", "stop_areas.json");
            GetAndSaveData<Route>(sncfApi, "routes", "routes.json");
            GetAndSaveData<StopPoint>(sncfApi, "stop_points", "stop_points.json");
        }

        static List<T> LoadSavedData<T>(string fileName)
        {
            var json = File.ReadAllText(fileName);
            List<T> result = JsonConvert.DeserializeObject<List<T>>(json);
            return result;
        }
        static void GetAndSaveData<T>(SncfApi sncfApi, string endpoint, string fileName) where T : new()
        {
            List<T> allItems = GetAllPagedResults<T>(sncfApi, endpoint);
            var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
            File.WriteAllText(fileName, json);
        }


        static List<T> GetAllPagedResults<T>(SncfApi sncfApi, string resourcePath, int chunckSize = DEFAULT_CHUNK_SIZE) where T : new()
        {
            if (chunckSize <= 0)
                throw new ArgumentOutOfRangeException("Chunck size must be > 0.");

            int numPage = 0;
            List<T> globalList = new List<T>();

            bool hasData = true;
            while (hasData)
            {
                PagedResult<T> pagedResult = sncfApi.GetPagedResult<T>(resourcePath, chunckSize, numPage);
                if (pagedResult.Results == null)
                {
                    hasData = false;
                }
                else
                {
                    globalList.AddRange(pagedResult.Results);
                    hasData = pagedResult.HasMoreData;
                    numPage++;
                }
            };

            return globalList;
        }


    }
}
