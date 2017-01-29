
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
        const string DATA_DIR = "data";
        static void Main(string[] args)
        {

            SncfRepository repo = new SncfRepository(DATA_DIR, 1000);
            SncfDataPack pack = repo.LoadDataPack();

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            // Warning : this does not respect API rules. Use at your own risk
            //repo.SaveStaticData();
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            string str2Find = "MEYRARGUES";
            repo.TestQueryWithStopName( str2Find);
            str2Find = "AIX-EN-PROVENCE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "MARSEILLE";
            repo.TestQueryWithStopName(str2Find);

            string idToFind = "stop_area:OCE:SA:87319012";
            repo.TestQueryWithId(idToFind);



        }


    }
}
