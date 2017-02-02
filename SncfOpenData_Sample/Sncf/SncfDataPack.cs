using NavitiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SncfOpenData
{
    public class SncfDataPack
    {
        public List<Line> Lines { get; set; }
        public List<Route> Routes { get; set; }
        public List<StopArea> StopAreas { get; set; }
        public List<StopPoint> StopPoints { get; set; }
        public Dictionary<string, List<RouteSchedule>> LineRouteSchedules { get; set; }
        public Dictionary<string, List<StopArea>> LinesStopAreas { get; internal set; }
        public Dictionary<string, List<StopArea>> RoutesStopAreas { get; internal set; }
        public Dictionary<string, int> IgnNodeByStopArea { get; internal set; }
        public Dictionary<int, HashSet<string>> StopAreaByIgnNode { get; internal set; }
    }
}
