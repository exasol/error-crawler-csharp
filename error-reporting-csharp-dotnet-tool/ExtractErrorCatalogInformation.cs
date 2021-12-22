﻿using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace error_reporting_csharp_dotnet_tool
{
    public class ExtractErrorCatalogInformation
    {

        public static async Task RunAsync(Options options)
        {
            SetMsBuild();

            string[] projectEntries;

            projectEntries = SetProjectEntries(options);

            ErrorCodeCollection errorCodeCollection = new ErrorCodeCollection();
            errorCodeCollection.ProjectShortTag = options.ProjectShortTag;
            errorCodeCollection.ProjectName = options.ProjectName;

            await CollectErrorCodes(projectEntries, errorCodeCollection);

            string generatedJSON = errorCodeCollection.GenerateJSON();
            File.WriteAllText("error_code_report.json", generatedJSON);
        }

        public static async Task CollectErrorCodes( string[] projectEntries, ErrorCodeCollection errorCodeCollection)
        {
            foreach (var project in projectEntries)
            {
                await ExtractExaErrorUsage(project, errorCodeCollection);
            }
        }

        private static string[] SetProjectEntries(Options options)
        {
            string[] projectEntries;
            if (options.ProjectEntry != null)
            {
                projectEntries = new string[1];
                projectEntries[0] = options.ProjectEntry;
            }
            else
            {
                string path = Directory.GetCurrentDirectory();
                projectEntries = Directory.GetFiles(path, "*.csproj",SearchOption.AllDirectories);
            }
            Console.WriteLine($"Found {projectEntries.Length} .csproj files");


            if (projectEntries.Length == 0)
            {
                throw new Exception("No projects found.");
            }
            return projectEntries;
        }

        private static async Task ExtractExaErrorUsage(string projectPath, ErrorCodeCollection errorCodeCollection)
        {
            

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);
                workspace.WorkspaceFailed += (o, e) => throw new Exception(e.Diagnostic.Kind.ToString() + " : " + e.Diagnostic.Message);
                //!!! this failed


                Console.WriteLine($"Loading project '{projectPath}'");

                //using msbuild workspace https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3

                // Attach progress reporter so we print projects as they are loaded.
                //var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                //Console.WriteLine($"Finished loading solution '{solutionPath}'");
                //There is also a open project async
                var project = await workspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());

                var compilation = await project.GetCompilationAsync();
                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.project?view=roslyn-dotnet-3.9.0


                var directoryName = Path.GetDirectoryName(projectPath);
                
                // Let's register mscorlib
                //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                
                var outputDirectory = $"{directoryName}\\bin\\debug";
                if (Directory.Exists(outputDirectory))
                {
                    var files = Directory.GetFiles(outputDirectory, "*.dll").ToList(); // You can also look for *.exe files

                    foreach (var f in files)
                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(f));
                }


                var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => $"{d.Id}: {d.GetMessage()}").ToList();

                if (errors.Any()) throw new Exception(string.Join("\r\n", errors));

                var projectDocuments = project.Documents;//.Where(doc => ! doc.Name.Contains("Assembly")) ;


                //we use the syntax walker to walk through each document
                //later on we can make this code execute concurrently

                foreach (var document in projectDocuments)
                {
                    await AnalyseDocument(document, errorCodeCollection,compilation);

                }
                //Documents produced from source generators are returned by GetSourceGeneratedDocumentsAsync(CancellationToken).
            }
        }

        private static void SetMsBuild()
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            //TODO: this might need more work later to make it more robust
            var instance = visualStudioInstances[0];

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);
        }

        private static async Task AnalyseDocument(Microsoft.CodeAnalysis.Document document, ErrorCodeCollection errorCodeCollection, Compilation compilation)
        {
            var tree = await document.GetSyntaxTreeAsync();
            // Get a root of the syntax tree
            var root = await tree.GetRootAsync();

            var semanticModel = compilation.GetSemanticModel(tree);

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
