using NavitiaSharp;
using SncfOpenData.IGN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SncfOpenData
{
    public class RoutePathResult
    {
        public Route Route { get; set; }
        public Line Line { get; set; }
        public RouteSchedule Schedule { get; set; }

        public List<Troncon> ResultTroncons { get; set; }
        public List<Noeud> Nodes { get; set; }
        public List<StopArea> StopAreas { get; set; }
        public Dictionary<StopArea,Noeud> StopAreasNode { get; set; }

        public StringBuilder Log { get; set; }
        public Exception Exception { get; set; }

        public string Temp_TronconsId
        {
            get
            {
                return "(" + String.Join<int>("),(", ResultTroncons.Select(p => p.Id)) + ")"; // check path clause
            }
        }


        public RoutePathResult()
        {
            ResultTroncons = new List<Troncon>();
            Nodes = new List<Noeud>();
            StopAreas = new List<StopArea>();
            StopAreasNode = new Dictionary<StopArea, Noeud>();
            Log = new StringBuilder();
        }

        public RoutePathResult(Route route, Line line) : base()
        {
            Route = route;
            Line = line;
        }
    }
}