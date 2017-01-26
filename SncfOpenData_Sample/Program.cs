
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
        static void Main(string[] args)
        {
            string sncfAuthKey = ConfigurationManager.AppSettings["SNCF_API_KEY"];
            SncfApi sncfApi = new SncfApi(sncfAuthKey);
            SaveLines(sncfApi);
            sncfApi.GetListResult<Line>("lines", "lines");
            Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");


            LoadStopAreas();
            SaveStopAreas(sncfApi);
            SaveLines(sncfApi);
            var stopAreasPAged = sncfApi.GetStopAreasPaged(1000, 0);
            var stopAreas = sncfApi.GetStopAreas(10, 0);

            var sa = sncfApi.GetStopArea("stop_area:OCE:SA:87113001");
            StopPoint sp = sncfApi.GetStopPoint("stop_point:OCE:SP:CorailIntercité-87113001");


        }

        static List<StopArea> LoadStopAreas()
        {
            var json = File.ReadAllText("stop_areas.json");
            List<StopArea> result = JsonConvert.DeserializeObject<List<StopArea>>(json);
            return result;
        }
        static void SaveStopAreas(SncfApi api)
        {
            List<StopArea> allItems = GetAllResults<StopArea>(api, "stop_areas");
            var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
            File.WriteAllText("stop_areas.json", json);

        }
        static void SaveLines(SncfApi api)
        {
            List<Line> allItems = GetAllResults<Line>(api, "lines");
            var json = JsonConvert.SerializeObject(allItems, Formatting.Indented);
            File.WriteAllText("lines.json", json);

        }
        static List<T> GetAllResults<T>(SncfApi sncfApi, string resourcePath, int chunckSize = 1000) where T : new()
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
                    //int maxPage = (int)Math.Floor((float)pagedResult.Pagination.TotalResult / pagedResult.Pagination.ItemsPerPage);

                    //numPage++;
                    //hasData = numPage <= maxPage;
                    hasData = pagedResult.HasMoreData;
                    numPage++;
                }
            };

            return globalList;
        }


    }
}
