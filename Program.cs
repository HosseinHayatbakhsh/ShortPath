using System;

namespace ShortestPath
{
    class Program
    {
        static void Main(string[] args)
        {
            ShortestPathService shortestPathService = new ShortestPathService();
            //string mapData = "..F;PW.;...";
            string mapData = "W.W..;P.WF.;.W.W.;.....;.WW.F";
            //string mapData = "W.W..;F.WF.;.W.W.;.....;.WW.P";
            //string mapData = "W.W..;P.WF.;WWWWW;.....;.WW.F";

            // Print out the map chart
            Console.WriteLine("Map chart:");
            foreach (char mapChar in mapData)
            {
                Console.Write(mapChar != ';' ? mapChar.ToString() + ' ' : '\n');
            }

            try
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("The shortest path is:");
                Console.WriteLine(shortestPathService.ShortestPath(mapData));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Running the shortest path function encountered following exception:\n");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
