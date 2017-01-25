
using NavitiaSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            var sa = sncfApi.GetStopArea("stop_area:OCE:SA:87113001");
            List<StopPoint> sp = sncfApi.GetStopPoint("stop_point:OCE:SP:CorailIntercité-87113001");
        }


    }
}
