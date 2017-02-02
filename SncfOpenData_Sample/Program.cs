using SncfOpenData.IGN;
using System;
using System.IO;

namespace SncfOpenData
{

    class Program
    {
        const string DATA_DIR_SNCF = @"..\..\..\Data\SNCF";
        const string DATA_DIR_IGN = @"..\..\..\Data\IGN";

        [STAThread()]
        static void Main(string[] args)
        {

            IGNRepository ignService = new IGNRepository(Path.Combine(DATA_DIR_IGN, "IGN_ROUTE500_SNCF.mdf"));
            SncfRepository repo = new SncfRepository(DATA_DIR_SNCF, 1000);
            CoreService core = new CoreService(repo, ignService);


            //select N.ID_RTE500 As ID_NOEUD
            //  ,T.ID_RTE500 as ID_TRONCON
            //from NOEUD_FERRE_2154 N
            //left
            //join TRONCON_VOIE_FERREE_2154 T

            //on N.geom2154.STIntersects(T.geom2154) = 1
            //order by ID_NOEUD


            //ShowStopAreasOnMap(repo, "POLYGON((5.2734375 43.259580971072275,5.41351318359375 43.1614915129406,5.4986572265625 43.295574211963746,5.5810546875 43.42936191764414,5.90789794921875 43.57678451504994,5.877685546875 43.74766111392921,5.88043212890625 43.86064850339098,5.62225341796875 43.75559702541283,5.4327392578125 43.670230832122314,5.27069091796875 43.58474304793296,5.23773193359375 43.431356514362626,5.2734375 43.259580971072275))");

            // Saves data identified as "static", ie: does not change often and can save remote Hits
            // Warning : this does not respect API rules. Use at your own risk
            //repo.SaveStaticData();
            // Line line = sncfApi.GetLine("line:OCE:SN-87276055-87276139");

            //repo.GetAndSavedRelatedData<Line, StopArea>(repo.Api, repo.DataPack.Lines, "lines/{id}/stop_areas", DATA_DIR_SNCF, "lines.stop_areas.json");
            //repo.GetAndSavedRelatedData<Route, StopArea>(repo.Api, repo.DataPack.Routes, "routes/{id}/stop_areas", DATA_DIR_SNCF, "routes.stop_areas.json");

            // TODO : get all route stop areas
            // we can then map to geo network with a little Dijkstra for each route
            // goal : for each network line, get all connected routes and lines 

            string idStopArea_Meyrargues = "stop_area:OCE:SA:87751370";
            string idStopArea_MarseilleStCharles = "stop_area:OCE:SA:87751008";
            string idStopArea_AixTgv = "stop_area:OCE:SA:87319012";

            var stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_Meyrargues);
            stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_MarseilleStCharles);
            stopPoints = repo.GetAllStopPointsForLinesStoppingAtStopArea(idStopArea_AixTgv);


            repo.TestQueryWithStopAreaId(idStopArea_Meyrargues);
            repo.TestQueryWithStopAreaId(idStopArea_AixTgv);

            string str2Find = "MEYRARGUES";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "AIX-EN-PROVENCE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "MARSEILLE";
            repo.TestQueryWithStopName(str2Find);
            str2Find = "NICE";
            repo.TestQueryWithStopName(str2Find);

        }




    }
}
