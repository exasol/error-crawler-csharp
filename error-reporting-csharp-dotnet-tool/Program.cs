using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
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
            var semanticModel = await document.GetSemanticModelAsync();

            var collector = new EECollector();
            collector.SemanticModel = semanticModel;
            collector.Visit(root);

            Console.WriteLine($@"Document: { document.Name } - Found {collector.lstInvocationExpressions.Count} elements");
        }

        class EECollector : CSharpSyntaxWalker
        {
            public SemanticModel SemanticModel {get;set;}

            //use base ctor to specify depth if necessary


            public List<InvocationExpressionSyntax> lstInvocationExpressions { get; } = new List<InvocationExpressionSyntax>();

            static bool IsExasolErrorCodeRelated(SemanticModel semanticModel,CSharpSyntaxNode node)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                var symbolContainingTypeStr = symbolInfo.Symbol.ContainingType.ToString();
                if (symbolContainingTypeStr == "Exasol.ErrorReporting.ExaError" || symbolContainingTypeStr == "Exasol.ErrorReporting.ErrorMessageBuilder")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                base.VisitInvocationExpression(node);

                var nodeExpression = node.Expression.ToString();
                if (nodeExpression.EndsWith("MessageBuilder") || nodeExpression.EndsWith("Message") ||
                    nodeExpression.EndsWith("TicketMitigation") || nodeExpression.EndsWith("Mitigation"))
                {
                    //if the method's called similar but isn't related do nothing with it
                    if (!IsExasolErrorCodeRelated(SemanticModel, node))
                    {
                        return;
                    }

                    if (node.Expression.ToString().EndsWith("MessageBuilder"))
                    {
                        //node.Expression.
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a construction helper function: {argList.Arguments[0]}");
                        lstInvocationExpressions.Add(node);


                        //IMethodSymbol  methodSymbol= symbolInfo.Symbol as IMethodSymbol;

                        //var contType = methodSymbol.ContainingType.ToString();
                        //Console.WriteLine($@"Symbol: {methodSymbol.ContainingSymbol}");
                        //Console.WriteLine(contType);
                    }
                    //this only triggers on the object one
                    else if (nodeExpression.EndsWith("Message"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a message function: {argList.Arguments[0]}");
                        var arg = node.ArgumentList;
                    }
                    else if (nodeExpression.EndsWith("TicketMitigation"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a mitigation function: {argList.Arguments[0]}");
                        var arg = node.ArgumentList;
                    }
                    else if (nodeExpression.EndsWith("Mitigation"))
                    {
                        var argList = node.ArgumentList;
                        Console.WriteLine($@"found a mitigation function: {argList.Arguments[0]}");
                        var arg = node.ArgumentList;
                    }
                }

            }

            public override void VisitLocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax node)
            {
                base.VisitLocalDeclarationStatement(node);
                Console.WriteLine($@"Declaration: {node.Declaration.Variables[0].Identifier}:");
                var init = node.Declaration.Variables[0].Initializer;
                Console.WriteLine($@"{init}:");
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

