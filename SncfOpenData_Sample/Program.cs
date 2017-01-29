
using NavitiaSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    class Program
    {
        const int DEFAULT_CHUNK_SIZE = 25;// 5000;
        const string DATA_DIR = "data";
        static void Main(string[] args)
        {
            string sncfAuthKey = ConfigurationManager.AppSettings["SNCF_API_KEY"];
            SncfApi sncfApi = new SncfApi(sncfAuthKey);


            //SaveStaticData(sncfApi);
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            List<Line> lines = LoadSavedData<Line>(DATA_DIR, "lines.json");
            List<Route> routes = LoadSavedData<Route>(DATA_DIR, "routes.json");
            List<StopArea> stopAreas = LoadSavedData<StopArea>(DATA_DIR, "stop_areas.json");
            List<StopPoint> stopPoints = LoadSavedData<StopPoint>(DATA_DIR, "stop_points.json");

            LoadAndProcessLineRouteSchedules();
            SaveLineRouteSchedules(sncfApi, lines);

            string str2Find = "MEYRARGUES";
            TestQueryWithStopName(lines, routes, stopAreas, stopPoints, str2Find);
            str2Find = "AIX-EN-PROVENCE";
            TestQueryWithStopName(lines, routes, stopAreas, stopPoints, str2Find);
            str2Find = "MARSEILLE";
            TestQueryWithStopName(lines, routes, stopAreas, stopPoints, str2Find);

            string idToFind = "stop_area:OCE:SA:87319012";
            TestQueryWithId(lines, routes, stopAreas, stopPoints, idToFind);

            //var saQuery = stopAreas.Where(sa => sa.Name.ToUpper().Contains("AIX-EN-PROVENCE")).ToList();


            // Saves data identified as "static", ie: does not change often and can save remote Hits
            //SaveStaticData(sncfApi);

            //var sa = sncfApi.GetStopArea("stop_area:OCE:SA:87113001");
            //StopPoint sp = sncfApi.GetStopPoint("stop_point:OCE:SP:CorailIntercité-87113001");


        }

        private static void SaveLineRouteSchedules(SncfApi sncfApi, List<Line> lines, int chunkSize = DEFAULT_CHUNK_SIZE)
        {
            if (!Directory.Exists("line.route_schedules"))
            {
                Directory.CreateDirectory("line.route_schedules");
            }
            foreach (var line in lines)
            {
                try
                {
                    List<RouteSchedule> allItems = GetAllPagedResults<RouteSchedule>(sncfApi, (n, p) => sncfApi.GetLineRouteSchedules(line.Id, n, p), chunkSize);
                    var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
                    File.WriteAllText($"line.route_schedules/{line.Id.Replace(":", ".")}.json", json);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error in SaveLineRouteSchedules for line {line.Id} : {ex.Message}.");
                }


            }
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

        /// <summary>
        /// Saves data identified as "static", ie: does not change often and can save remote Hits
        /// Important : this is not authorized by SNCF api terms, and is there for tests purposes
        /// </summary>
        /// <param name="sncfApi"></param>
        private static void SaveStaticData(SncfApi sncfApi, string dataDir)
        {
            GetAndSaveData<Line>(sncfApi, "lines", dataDir, "lines.json");
            GetAndSaveData<StopArea>(sncfApi, "stop_areas", dataDir, "stop_areas.json");
            GetAndSaveData<Route>(sncfApi, "routes", dataDir, "routes.json");
            GetAndSaveData<StopPoint>(sncfApi, "stop_points", dataDir, "stop_points.json");
        }

        static List<T> LoadSavedData<T>(string dataDir, string fileName)
        {
            CheckDataDirExists(dataDir);

            var json = File.ReadAllText(Path.Combine(dataDir, fileName));
            List<T> result = JsonConvert.DeserializeObject<List<T>>(json);
            return result;
        }
        static void CheckDataDirExists(string dataDir)
        {
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }
        static void GetAndSaveData<T>(SncfApi sncfApi, string endpoint, string dataDir, string fileName) where T : new()
        {
            CheckDataDirExists(dataDir);

            List<T> allItems = GetAllPagedResults<T>(sncfApi, endpoint);
            var json = JsonConvert.SerializeObject(allItems, Formatting.None);
            File.WriteAllText(Path.Combine(dataDir, fileName), json);
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
        static List<T> GetAllPagedResults<T>(SncfApi sncfApi, Func<int, int, PagedResult<T>> apiFunc, int chunckSize = DEFAULT_CHUNK_SIZE) where T : new()
        {
            if (chunckSize <= 0)
                throw new ArgumentOutOfRangeException("Chunck size must be > 0.");

            int numPage = 0;
            List<T> globalList = new List<T>();

            bool hasData = true;
            while (hasData)
            {
                PagedResult<T> pagedResult = apiFunc(chunckSize, numPage);
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
