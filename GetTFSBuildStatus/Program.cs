using System;
using System.Linq;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;

namespace GetTFSBuildStatus
{
    class Program
    {
        /// <summary>
        /// Displays status of latest build, 
        /// and if the build is queued or in progress, will display status of queued builld. 
        /// </summary>
        /// <param name="args">[Team Project Collection Url, Project Name, Build def name]</param>
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Required parameters not received...");
                Console.ForegroundColor = ConsoleColor.Yellow; 
                Console.WriteLine("Usage:");
                Console.ForegroundColor = ConsoleColor.White; 
                Console.WriteLine("\"GetTFSBuildStatus [TPC Url] [Project name] [Build def name]\"");
                Console.ForegroundColor = ConsoleColor.Yellow; 
                Console.WriteLine("Example:");
                Console.ForegroundColor = ConsoleColor.White; 
                Console.WriteLine("\"GetTFSBuildStatus http://tfs-server.domain.com:8080/tfs/DefaultCollection MyTeamProject MyApp-CI-Build01\"");
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }

            try
            {
                var collectionUriArg = args[0];
                var projectName = args[1];
                var buildDefName = args[2];

                var collectionUri = new Uri(collectionUriArg);
                var tfsServer = new TfsTeamProjectCollection(collectionUri);
                var buildserver = (IBuildServer)tfsServer.GetService(typeof(IBuildServer));

                Console.WriteLine("\nQuerying build server for build: " + buildDefName);
                var def = buildserver.QueryBuildDefinitions(projectName).SingleOrDefault(d => d.Name.Equals(buildDefName, StringComparison.OrdinalIgnoreCase));
                var spec = buildserver.CreateBuildQueueSpec(projectName, buildDefName);

                if (def == null) return;
                var lastbuild = def.QueryBuilds().Last();
                if (lastbuild != null)
                {
                    Console.WriteLine("\nLATEST BUILD...");
                    Console.WriteLine("Build Name:\t\t" + buildDefName);
                    Console.WriteLine("Start Time:\t\t" + lastbuild.StartTime);
                    if (lastbuild.BuildFinished) Console.WriteLine("Finish Time:\t\t" + lastbuild.FinishTime);
                    if (!String.IsNullOrEmpty(lastbuild.Quality)) Console.WriteLine("Quality:\t\t" + lastbuild.Quality);
                    Console.WriteLine("Test Status:\t\t" + lastbuild.TestStatus);
                    Console.WriteLine("Build Status:\t\t" + lastbuild.Status);
                }

                var queued = buildserver.QueryQueuedBuilds(spec).QueuedBuilds;
                if (queued.Length <= 0) return;
                Console.WriteLine("\nQUEUED BUILD...");
                var queuedBuild = queued.First();
                Console.WriteLine("\nBuild Name:\t" + buildDefName);
                Console.WriteLine("Queued Time:\t" + queuedBuild.QueueTime);
                Console.WriteLine("Status:\t" + queuedBuild.Status);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
