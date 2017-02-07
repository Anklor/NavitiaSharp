using NavitiaSharp;
using Newtonsoft.Json;
using SncfOpenData.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class SncfRepository
    {
        private const string LINE_ROUTE_SCHEDULES_DIR = "line.route_schedules";
        readonly string _dataDirectory;
        readonly SncfApi _api;
        public SncfApi Api
        {
            get { return _api; }
        }

        #region Data 

        private List<Line> _lines;
        public List<Line> Lines
        {
            get
            {
                if (_lines == null) _lines = LoadSavedDataList<Line>("lines.json");
                return _lines;
            }
            internal set { _lines = value; }
        }

        private List<Route> _routes;
        public List<Route> Routes
        {
            get
            {
                if (_routes == null) _routes = LoadSavedDataList<Route>("routes.json");
                return _routes;
            }
            internal set { _routes = value; }
        }
        private List<StopArea> _stopAreas;
        public List<StopArea> StopAreas
        {
            get
            {
                if (_stopAreas == null) _stopAreas = LoadSavedDataList<StopArea>("stop_areas.json");
                return _stopAreas;
            }
            internal set { _stopAreas = value; }
        }
        private List<StopPoint> _stopPoints;
        public List<StopPoint> StopPoints
        {
            get
            {
                if (_stopPoints == null) _stopPoints = LoadSavedDataList<StopPoint>("stop_points.json");
                return _stopPoints;
            }
            internal set { _stopPoints = value; }
        }

        private Dictionary<string, List<RouteSchedule>> _lineRouteSchedules;
        public Dictionary<string, List<RouteSchedule>> LineRouteSchedules
        {
            get
            {
                //pack.LineRouteSchedules = pack.Lines.Select(line => new { lineId = line.Id, schedules = GetLineRouteSchedules(line, false) })
                //                                      .ToDictionary(a => a.lineId, a => a.schedules);

                if (_lineRouteSchedules == null) _lineRouteSchedules = LoadSavedData<Dictionary<string, List<RouteSchedule>>>("linesroutesschedules.json");
                return _lineRouteSchedules;
            }
            internal set { _lineRouteSchedules = value; }
        }
        private Dictionary<string, List<RouteSchedule>> _routeRouteSchedules;

        private Dictionary<string, List<StopArea>> _linesStopAreas;
        public Dictionary<string, List<StopArea>> LinesStopAreas
        {
            get
            {
                if (_linesStopAreas == null) _linesStopAreas = LoadSavedData<Dictionary<string, List<StopArea>>>("lines.stop_areas.json");
                return _linesStopAreas;
            }
            internal set { _linesStopAreas = value; }
        }
        private Dictionary<string, List<StopArea>> _routesStopAreas;
        public Dictionary<string, List<StopArea>> RoutesStopAreas
        {
            get
            {
                if (_routesStopAreas == null) _routesStopAreas = LoadSavedData<Dictionary<string, List<StopArea>>>("routes.stop_areas.json");
                return _routesStopAreas;
            }
            internal set { _routesStopAreas = value; }
        }
        private Dictionary<string, int> _ignNodeByStopArea;
        public Dictionary<string, int> IgnNodeByStopArea
        {
            get
            {
                if (_ignNodeByStopArea == null) _ignNodeByStopArea = LoadSavedDataList<StopAreaIGN>("stopAreasIgn.json").ToDictionary(kvp => kvp.StopAreaId, kvp => kvp.IdNoeud);
                return _ignNodeByStopArea;
            }
            internal set { _ignNodeByStopArea = value; }
        }
        private Dictionary<int, HashSet<string>> _stopAreaByIgnNode;
        public Dictionary<int, HashSet<string>> StopAreaByIgnNode
        {
            get
            {
                if (_stopAreaByIgnNode == null)
                {
                    _stopAreaByIgnNode = new Dictionary<int, HashSet<string>>();
                    foreach (var kvp in IgnNodeByStopArea.Where(kvp => kvp.Value != 0))
                    {
                        if (_stopAreaByIgnNode.ContainsKey(kvp.Value) == false)
                        {
                            _stopAreaByIgnNode[kvp.Value] = new HashSet<string>();
                        }
                        _stopAreaByIgnNode[kvp.Value].Add(kvp.Key);
                    }
                }

                return _stopAreaByIgnNode;
            }
            internal set { _stopAreaByIgnNode = value; }
        }

        #endregion

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


        public List<T> LoadSavedDataList<T>(string fileName)
        {
            string fullFileName = Path.Combine(_dataDirectory, fileName);
            if (File.Exists(fullFileName))
            {
                var json = File.ReadAllText(fullFileName);
                List<T> result = JsonConvert.DeserializeObject<List<T>>(json);
                return result;
            }
            else
            {
                return new List<T>();
            }
        }
        public T LoadSavedData<T>(string fileName)
        {
            string fullFileName = Path.Combine(_dataDirectory, fileName);
            if (File.Exists(fullFileName))
            {
                var json = File.ReadAllText(fullFileName);
                T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            else
            {
                return default(T);
            }
        }

        private void CheckDirExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public List<RouteSchedule> GetRouteSchedules(Route route, bool fromApi = false)
        {
            List<RouteSchedule> allItems = null;
            if (route != null)
            {
                if (fromApi)
                {
                    allItems = GetAllPagedResults<RouteSchedule>(_api, (n, p) => _api.GetRouteRouteSchedules(route.Id, n, p), ChunckSize);
                }
                else
                {
                    if (_routeRouteSchedules == null) _routeRouteSchedules = new Dictionary<string, List<RouteSchedule>>();
                    if (!_routeRouteSchedules.ContainsKey(route.Id))
                    {
                        string fileName = "route_schedules\\route.{id}.route_schedules.json".Replace("{id}", SafeFileId(route.Id));
                        _routeRouteSchedules[route.Id] = LoadSavedDataList<RouteSchedule>(fileName);
                    }
                    return _routeRouteSchedules[route.Id];
                }
            }
            return allItems;
        }

        
        
        public RouteSchedule GetRouteScheduleForRoute(List<RouteSchedule> routeSchedules, Route route)
        {
            var schedules = (from schedule in routeSchedules
                             where route.Id == GetRouteIdFromSchedule(schedule) && schedule.DisplayInformations.Direction == route.Direction.StopArea.Label
                             select schedule).ToList();

            Debug.Assert(schedules.Count > 0, "No route schedules. Check destination match with stop area label !");
            Debug.Assert(schedules.Count == 1, "Multiple route schedules !");
            return schedules.First();
        }
        public string GetRouteIdFromSchedule(RouteSchedule schedule)
        {
            Link routeLink = schedule.Links.Where(link => link.Type == "route").FirstOrDefault();
            if (routeLink == null)
            {
                return null;
            }
            else
            {
                return routeLink.Id;
            }
        }

        private void SaveLineRouteSchedules(SncfApi sncfApi, List<Line> lines, string dataDir, int chunkSize = DEFAULT_CHUNK_SIZE)
        {
            string schedulesDir = Path.Combine(dataDir, LINE_ROUTE_SCHEDULES_DIR);
            CheckDirExists(schedulesDir);

            foreach (var line in lines)
            {
                try
                {
                    List<RouteSchedule> allItems = GetAllPagedResults<RouteSchedule>(sncfApi, (n, p) => sncfApi.GetLineRouteSchedules(line.Id, n, p), chunkSize);
                    var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
                    File.WriteAllText(Path.Combine(schedulesDir, $"{SafeFileId(line.Id)}.json"), json);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error in SaveLineRouteSchedules for line {line.Id} : {ex.Message}.");
                }


            }
        }

        private string SafeFileId(string navitiaId)
        {
            return navitiaId.Replace(":", ".");
        }

        public void TestQueryWithStopName(string str2Find)
        {
            var saQuery = StopAreas.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var spQuery = StopPoints.Where(obj => obj.Name.ToUpper().Contains(str2Find)).ToList();
            var linesQuery = Lines.Where(obj => obj.Routes != null && obj.Routes.Any(r => r.Direction.Name.ToUpper().Contains(str2Find))).ToList();
            var routesQuery = Routes.Where(obj => obj.Direction.Name.ToUpper().Contains(str2Find)).ToList();
        }
        public void TestQueryWithId(string idToFind)
        {
            var saQuery = StopAreas.Where(obj => obj.Id == idToFind).ToList();
            var linesQuery = Lines.Where(obj => obj.Id == idToFind).ToList();
            var routesQuery = Routes.Where(obj => obj.Id == idToFind).ToList();
            var spQuery = StopPoints.Where(obj => obj.Id == idToFind).ToList();
        }

        public void TestQueryWithStopAreaId(string idToFind)
        {
            var saQuery = StopAreas.Where(obj => obj.Id == idToFind).ToList();
            var linesQuery = Lines.Where(obj => obj.Routes != null && obj.Routes.Any(r => r.Direction.StopArea.Id == idToFind)).ToList();
            var routesQuery = Routes.Where(obj => obj.Direction.StopArea.Id == idToFind).ToList();
            var spQuery = StopPoints.Where(obj => obj.StopArea.Id == idToFind).ToList();

            var routeSchedulesQueryFromMemory = LineRouteSchedules.SelectMany(rs => rs.Value).Where(rs => rs.Table.Rows.Any(r => r.StopPoint.StopArea.Id == idToFind)).ToList();

            List<Table> tables = new List<Table>();
            List<List<StopPoint>> listStopPoints = new List<List<StopPoint>>();
            HashSet<StopPoint> stopPoints = new HashSet<StopPoint>();
            foreach (RouteSchedule routeSchedule in routeSchedulesQueryFromMemory)
            {
                // Get all stop points. Line is stopping at stop area
                var stopPointsQuery = routeSchedule.Table.Rows.Select(r => r.StopPoint);

                stopPoints.UnionWith(stopPointsQuery);
                listStopPoints.Add(new List<StopPoint>(stopPointsQuery));
                tables.Add(routeSchedule.Table);
            }
            HashSet<string> journeys = new HashSet<string>();
            routeSchedulesQueryFromMemory.ForEach(rs => journeys.UnionWith(ExtractVehiculeJourneysFromRouteSchedule(rs)));
        }

        public Dictionary<string, HashSet<StopPoint>> GetAllStopPointsForLinesStoppingAtStopArea(string idToFind)
        {
            var saQuery = StopAreas.Where(obj => obj.Id == idToFind).ToList();
            Debug.Assert(saQuery.Any(), "No stop area with this Id !");

            //var routeSchedulesQueryFromMemory = LineRouteSchedules.SelectMany(rs => rs.Value).Where(rs => rs.Table.Rows.Any(r => r.StopPoint.StopArea.Id == idToFind)).ToList();

            Dictionary<string, HashSet<StopPoint>> v_ret = new Dictionary<string, HashSet<StopPoint>>();
            List<Table> tables = new List<Table>();
            List<List<StopPoint>> listStopPoints = new List<List<StopPoint>>();
            HashSet<StopPoint> stopPoints = new HashSet<StopPoint>();
            foreach (KeyValuePair<string, List<RouteSchedule>> routeSchedulesByLineId in LineRouteSchedules)
            {
                foreach (RouteSchedule routeSchedule in routeSchedulesByLineId.Value)
                {
                    if (routeSchedule.Table.Rows.Any(r => r.StopPoint.StopArea.Id == idToFind))
                    {
                        // Get all stop points. Line is stopping at stop area
                        var stopPointsQuery = routeSchedule.Table.Rows.Select(r => r.StopPoint);

                        stopPoints.UnionWith(stopPointsQuery);
                        listStopPoints.Add(new List<StopPoint>(stopPointsQuery));
                        tables.Add(routeSchedule.Table);

                        if (!v_ret.ContainsKey(routeSchedulesByLineId.Key))
                        {
                            v_ret.Add(routeSchedulesByLineId.Key, new HashSet<StopPoint>());
                        }
                        v_ret[routeSchedulesByLineId.Key].UnionWith(stopPoints);
                    }
                }
            }

            return v_ret;
        }
        private HashSet<string> ExtractVehiculeJourneysFromRouteSchedule(RouteSchedule routeSchedule)
        {
            //Table / Links / Type "vehicle_journey"

            HashSet<string> journeys = new HashSet<string>();
            foreach (var header in routeSchedule.Table.Headers)
            {
                foreach (var link in header.Links)
                {
                    if (link.Type == "vehicle_journey")
                    {
                        journeys.Add(link.Id);
                    }
                    if (link.Type.StartsWith("trip"))
                    {
                        journeys.Add(link.Id);
                    }
                }
            }
            foreach (var row in routeSchedule.Table.Rows)
            {
                foreach (var dt in row.DateTimes)
                {
                    foreach (var link in dt.Links)
                    {
                        if (link.Type.StartsWith("trip"))
                        {
                            journeys.Add(link.Id);
                        }
                    }
                }
                foreach (var link in row.StopPoint.Links)
                {
                    if (link.Type.StartsWith("trip"))
                    {
                        journeys.Add(link.Id);
                    }
                }
            }
            return journeys;

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
            var lines = GetAndSaveData<Line>(sncfApi, "lines", dataDir, "lines.json");
            GetAndSaveData<StopArea>(sncfApi, "stop_areas", dataDir, "stop_areas.json");
            GetAndSaveData<Route>(sncfApi, "routes", dataDir, "routes.json");
            GetAndSaveData<StopPoint>(sncfApi, "stop_points", dataDir, "stop_points.json");
            GetAndSavedRelatedData<Line, StopArea>(sncfApi, lines, "lines/{id}/stop_areas", dataDir, "lines.stop_areas.json");

        }

        public Dictionary<string, List<TRelated>> GetAndSavedRelatedData<TBase, TRelated>(SncfApi sncfApi,
                                                                                            List<TBase> baseResourceList,
                                                                                            string patternPathToRelatedResource,
                                                                                            string dataDirectory,
                                                                                            string outputFileName) where TBase : ApiResourceBase, new()
                                                                                                                   where TRelated : new()
        {
            //Dictionary<string, List<TRelated>> dic = new Dictionary<string, List<TRelated>>();
            ConcurrentDictionary<string, List<TRelated>> dic = new ConcurrentDictionary<string, List<TRelated>>();

            Parallel.ForEach(baseResourceList, baseItem =>
            {
                try
                {
                    List<TRelated> allRelatedItems = GetAllPagedResults<TRelated>(sncfApi, patternPathToRelatedResource.Replace("{id}", baseItem.Id), 100);
                    dic.AddOrUpdate(baseItem.Id, allRelatedItems, AddRelatedItem);
                }
                catch (KeyNotFoundException)
                {
                    dic.AddOrUpdate(baseItem.Id, new List<TRelated>(), AddRelatedItem);
                    Trace.TraceWarning("GetAndSavedRelatedData. Not found.");
                }
                catch (Exception v_ex)
                {
                    Trace.TraceWarning($"GetAndSavedRelatedData. Error : {v_ex.Message}");
                }

            }
            );
            //var json = JsonConvert.SerializeObject(dic, Formatting.None);
            //File.WriteAllText(Path.Combine(dataDirectory, outputFileName), json);

            Dictionary<string, List<TRelated>> outDic = new Dictionary<string, List<TRelated>>();
            outDic = dic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var json = JsonConvert.SerializeObject(outDic, Formatting.None);
            File.WriteAllText(Path.Combine(dataDirectory, outputFileName), json);
            return outDic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <typeparam name="TRelated"></typeparam>
        /// <param name="sncfApi"></param>
        /// <param name="baseResourceList"></param>
        /// <param name="patternPathToRelatedResource"></param>
        /// <param name="dataDirectory"></param>
        /// <param name="outputFileNamePattern">Should contain {id}</param>
        /// <returns></returns>
        public void GetAndSavedRelatedData_Detail<TBase, TRelated>(SncfApi sncfApi,
                                                                                            List<TBase> baseResourceList,
                                                                                            string patternPathToRelatedResource,
                                                                                            string dataDirectory,
                                                                                            string outputFileNamePattern) where TBase : ApiResourceBase, new()
                                                                                                                   where TRelated : new()
        {

            if (!outputFileNamePattern.Contains("{id}"))
            {
                throw new ArgumentException("outputFileNamePattern should contain {id}.");
            }
            string v_directory = Path.GetDirectoryName(Path.Combine(dataDirectory, outputFileNamePattern.Replace("{id}", "")));
            Directory.CreateDirectory(v_directory);

            Parallel.ForEach(baseResourceList, baseItem =>
            //foreach (var baseItem in baseResourceList)
            {
                try
                {
                    List<TRelated> allRelatedItems = GetAllPagedResults<TRelated>(sncfApi, patternPathToRelatedResource.Replace("{id}", baseItem.Id), 100);

                    var json = JsonConvert.SerializeObject(allRelatedItems, Formatting.Indented);
                    File.WriteAllText(Path.Combine(dataDirectory, outputFileNamePattern.Replace("{id}", SafeFileId(baseItem.Id))), json);
                }
                catch (KeyNotFoundException)
                {
                    Trace.TraceWarning($"GetAndSavedRelatedData. Not found for {baseItem.Id}");
                }
                catch (Exception v_ex)
                {
                    Trace.TraceWarning($"GetAndSavedRelatedData. Error for {baseItem.Id} : {v_ex.Message}");
                }

            }
            );
        }

        private List<TRelated> AddRelatedItem<TRelated>(string arg1, List<TRelated> arg2) where TRelated : new()
        {
            return arg2;
        }

        List<T> GetAndSaveData<T>(SncfApi sncfApi, string endpoint, string dataDir, string fileName) where T : new()
        {
            CheckDirExists(dataDir);

            List<T> allItems = GetAllPagedResults<T>(sncfApi, endpoint);
            var json = JsonConvert.SerializeObject(allItems, Formatting.None);
            File.WriteAllText(Path.Combine(dataDir, fileName), json);

            return allItems;
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
