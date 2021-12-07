using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace error_reporting_csharp_dotnet_tool
{
    //https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create
    class Program
    {
        static async Task Main(string[] args)
        {
            //https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create

            //MSBuildLocator.RegisterDefaults();

            string projectPath;
            if (args.Length == 1)
            {
                projectPath = args[0];
            }
            else
            {
                string path = Directory.GetCurrentDirectory();
                string[] fileEntries = Directory.GetFiles(path, "*.csproj");
                projectPath = fileEntries[0];
            }
            await ExtractExaErrorUsage(projectPath);
        }
            private static async Task ExtractExaErrorUsage(string projectPath)
            {
                // Attempt to set the version of MSBuild.
                var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
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
                    await AnalyseDocument(document);

                }
                //Documents produced from source generators are returned by GetSourceGeneratedDocumentsAsync(CancellationToken).
            }
            }

        private static async Task AnalyseDocument(Microsoft.CodeAnalysis.Document document)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var root = syntaxTree.GetCompilationUnitRoot();

            //we might need this for further or more elaborate analysis later on
            //var semanticModel = document.GetSemanticModelAsync();

            var collector = new EECollector();
            collector.Visit(root);

            Console.WriteLine($@"Document: { document.Name } - Found { collector.lstObjectCreationExpressions.Count } and {collector.lstInvocationExpressions.Count} elements");
        }

        class EECollector : CSharpSyntaxWalker
        {

            public List<ObjectCreationExpressionSyntax> lstObjectCreationExpressions { get; } = new List<ObjectCreationExpressionSyntax>();
            public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                if (node.Type.ToString().Contains("ErrorMessageBuilder"))
                {
                    Console.WriteLine("found a new!");
                    lstObjectCreationExpressions.Add(node);
                }
                base.VisitObjectCreationExpression(node);

            }

            public List<InvocationExpressionSyntax> lstInvocationExpressions { get; } = new List<InvocationExpressionSyntax>();
            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression.ToString().Contains("ExaError.MessageBuilder"))
                {
                    Console.WriteLine("found the helper function!");
                    lstInvocationExpressions.Add(node);
                }
                base.VisitInvocationExpression(node);
            }


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

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}

