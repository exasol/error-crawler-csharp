using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace error_reporting_csharp_dotnet_tool
{
    //https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create
    partial class Program
    {
        static async Task Main(string[] args)
        {
            //https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create

            string[] projectEntries;

            if (args.Length == 1)
            {
                projectEntries = new string[1];
                projectEntries[0] = args[0];
            }
            else
            {
                string path = Directory.GetCurrentDirectory();
                projectEntries = Directory.GetFiles(path, "*.csproj");
            }

            ErrorCodeCollection errorCodeCollection = new ErrorCodeCollection();
            //TODO: read this out from command line and block if it's not provided
            errorCodeCollection.ProjectShortTag = "ECC";
            errorCodeCollection.ProjectName = "place holder project name"
            errorCodeCollection.ProjectVersion = "1.0.0";

            foreach (var project in projectEntries ){
                await ExtractExaErrorUsage(project,errorCodeCollection);
            }

            errorCodeCollection.BuildJSON();
            
        }
        private static async Task ExtractExaErrorUsage(string projectPath,ErrorCodeCollection errorCodeCollection)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            //TODO: this might need more work later to make it more robust
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);


                Console.WriteLine($"Loading project '{projectPath}'");

                //using msbuild workspace https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3

                // Attach progress reporter so we print projects as they are loaded.
                //var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                //Console.WriteLine($"Finished loading solution '{solutionPath}'");
                //There is also a open project async
                var project = await workspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());

                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.project?view=roslyn-dotnet-3.9.0


                var projectDocuments = project.Documents;//.Where(doc => ! doc.Name.Contains("Assembly")) ;

                //we use the syntax walker to walk through each document
                //later on we can make this code execute concurrently
                foreach (var document in projectDocuments)
                {
                    await AnalyseDocument(document, errorCodeCollection);

                }
                //Documents produced from source generators are returned by GetSourceGeneratedDocumentsAsync(CancellationToken).
            }
        }

        private static async Task AnalyseDocument(Microsoft.CodeAnalysis.Document document, ErrorCodeCollection errorCodeCollection)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var root = syntaxTree.GetCompilationUnitRoot();

            //we might need this for further or more elaborate analysis later on
            var semanticModel = await document.GetSemanticModelAsync();

            var errorCodeCrawler = new ErrorCodeCrawler(semanticModel, errorCodeCollection);

            errorCodeCrawler.Visit(root);

            Console.WriteLine($@"Document: { document.Name } - Done");
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }
    }
}

