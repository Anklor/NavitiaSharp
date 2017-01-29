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
    public class SncfRepository
    {
        readonly string _dataDirectory;
        readonly SncfApi _api;
        public SncfApi Api
        {
            get { return _api; }
        }

        private SncfDataPack _dataPack;
        public SncfDataPack DataPack
        {
            get
            {
                if (_dataPack == null)
                {
                    _dataPack = LoadDataPack();
                }
                return _dataPack;
            }
        }

        const int DEFAULT_CHUNK_SIZE = 25;// 5000;

        public int ChunckSize { get; set; }

        public SncfRepository(string dataDirectory, int chunckSize = DEFAULT_CHUNK_SIZE)
        {
            _dataDirectory = dataDirectory;
            ChunckSize = chunckSize;
            CheckDirExists(_dataDirectory);
            string sncfAuthKey = ConfigurationManager.AppSettings["SNCF_API_KEY"];
            _api = new SncfApi(sncfAuthKey);
        }

        public SncfDataPack LoadDataPack()
        {
            SncfDataPack pack = new SncfDataPack();
            pack.Lines = LoadSavedData<Line>("lines.json");
            pack.Routes = LoadSavedData<Route>("routes.json");
            pack.StopAreas = LoadSavedData<StopArea>("stop_areas.json");
            pack.StopPoints = LoadSavedData<StopPoint>("stop_points.json");

            return pack;
        }

        private List<T> LoadSavedData<T>(string fileName)
        {
            var json = File.ReadAllText(Path.Combine(_dataDirectory, fileName));
            List<T> result = JsonConvert.DeserializeObject<List<T>>(json);
            return result;
        }

        private void CheckDirExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        private void SaveLineRouteSchedules(SncfApi sncfApi, List<Line> lines, string dataDir, int chunkSize = DEFAULT_CHUNK_SIZE)
        {
            string schedulesDir = Path.Combine(dataDir, "line.route_schedules");
            CheckDirExists(schedulesDir);

            foreach (var line in lines)
            {
                try
                {
                    List<RouteSchedule> allItems = GetAllPagedResults<RouteSchedule>(sncfApi, (n, p) => sncfApi.GetLineRouteSchedules(line.Id, n, p), chunkSize);
                    var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
                    File.WriteAllText(Path.Combine(schedulesDir, $"{line.Id.Replace(":", ".")}.json"), json);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error in SaveLineRouteSchedules for line {line.Id} : {ex.Message}.");
                }


            }
        }

        public void TestQueryWithStopName(string str2Find)
        {
            var saQuery = _dataPack.StopAreas.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var spQuery = _dataPack.StopPoints.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var linesQuery = _dataPack.Lines.Where(obj => obj.Routes != null && obj.Routes.Any(r => r.Direction.Name.ToUpper().Contains(str2Find))).ToList();
            var routesQuery = _dataPack.Routes.Where(obj => obj.Direction.Name.ToUpper().Contains(str2Find)).ToList();
        }
        public void TestQueryWithId(string idToFind)
        {
            var saQuery = _dataPack.StopAreas.Where(obj => obj.Id == idToFind).ToList();
            var linesQuery = _dataPack.Lines.Where(obj => obj.Id == idToFind).ToList();
            var routesQuery = _dataPack.Routes.Where(obj => obj.Id == idToFind).ToList();
            var spQuery = _dataPack.StopPoints.Where(obj => obj.Id == idToFind).ToList();
        }


        public void SaveStaticData()
        {
            SaveStaticData(_api, _dataDirectory);
        }
        /// <summary>
        /// Saves data identified as "static", ie: does not change often and can save remote Hits
        /// Important : this is not authorized by SNCF api terms, and is there for tests purposes
        /// </summary>
        /// <param name="sncfApi"></param>
        private void SaveStaticData(SncfApi sncfApi, string dataDir)
        {
            GetAndSaveData<Line>(sncfApi, "lines", dataDir, "lines.json");
            GetAndSaveData<StopArea>(sncfApi, "stop_areas", dataDir, "stop_areas.json");
            GetAndSaveData<Route>(sncfApi, "routes", dataDir, "routes.json");
            GetAndSaveData<StopPoint>(sncfApi, "stop_points", dataDir, "stop_points.json");
        }

        void GetAndSaveData<T>(SncfApi sncfApi, string endpoint, string dataDir, string fileName) where T : new()
        {
            CheckDirExists(dataDir);

            List<T> allItems = GetAllPagedResults<T>(sncfApi, endpoint);
            var json = JsonConvert.SerializeObject(allItems, Formatting.None);
            File.WriteAllText(Path.Combine(dataDir, fileName), json);
        }


        List<T> GetAllPagedResults<T>(SncfApi sncfApi, string resourcePath, int chunckSize = DEFAULT_CHUNK_SIZE) where T : new()
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
        List<T> GetAllPagedResults<T>(SncfApi sncfApi, Func<int, int, PagedResult<T>> apiFunc, int chunckSize = DEFAULT_CHUNK_SIZE) where T : new()
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
