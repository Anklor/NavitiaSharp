
using NavitiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    class Program
    {
        static void Main(string[] args)
        {
            SncfApi sncfApi = new SncfApi("YOUR_API_KEY");
            var sa = sncfApi.GetStopArea("stop_area:OCE:SA:87113001");
            StopPointCollection sp = sncfApi.GetStopPoint("stop_point:OCE:SP:CorailIntercité-87113001");
        }
    }
}
