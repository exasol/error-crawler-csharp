using Microsoft.Build.Locator;
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
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Kind + " : " + e.Diagnostic.Message);

                Console.WriteLine($"Loading project '{projectPath}'");

                //using msbuild workspace https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3

                // Attach progress reporter so we print projects as they are loaded.
                var project = await workspace.OpenProjectAsync(projectPath, new ConsoleProgressReporter());

                var compilation = await project.GetCompilationAsync();
                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.project?view=roslyn-dotnet-3.9.0


                var directoryName = Path.GetDirectoryName(projectPath);

                //on loading the dependencies: https://www.michalkomorowski.com/2017/03/why-i-hate-roslyn.html
                //TODO: This is pretty makeshift: I'm not sure if this is 100% fireproof, we'll see later.

                // Let's register mscorlib (NOTE: turned this off since it seemed to conflict/have similar with loading the dlls)
                //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
                Console.WriteLine($"Loading assemblies: '{directoryName}'");
                var outputDirectory = $"{directoryName}\\bin\\debug";
                if (Directory.Exists(outputDirectory))
                {
                    var files = Directory.GetFiles(outputDirectory, "*.dll", SearchOption.AllDirectories).ToList(); // You can also look for *.exe files

                    foreach (var f in files)
                        compilation = compilation.AddReferences(MetadataReference.CreateFromFile(f));
                }

                CheckCompilationDiagnostics(compilation);

                var projectDocuments = project.Documents;//.Where(doc => ! doc.Name.Contains("Assembly")) ;

                //we use the syntax walker to walk through each document
                //later on we can make this code execute concurrently
                foreach (var document in projectDocuments)
                {
                    await AnalyseDocument(document, errorCodeCollection, compilation);

                }
                //Documents produced from source generators are returned by GetSourceGeneratedDocumentsAsync(CancellationToken).
            }
        }

        private static void CheckCompilationDiagnostics(Compilation compilation)
        {
            Console.WriteLine($"Checking project compilation diagnostics");
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => $"{d.Id}: {d.GetMessage()}").ToList();

            if (errors.Any()) throw new Exception(string.Join("\r\n", errors));
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

    }
}
