using CommandLine;

namespace error_reporting_csharp_dotnet_tool
{

        public class Options
        {
            [Option('t', "tag", Required = true, HelpText = "Provide a project short tag, e.g: \"ERC\".")]
            public string ProjectShortTag { get; set; }

            [Option('p', "project entry", Required = false, HelpText = "Provide a specific project entry, e.g: \"projectfolder\\project.csproj\".")]
            public string ProjectEntry { get; set; }

            [Option('n', "project name", Required = false, HelpText = "Provide a project name e.g: \"error-crawler-csharp\"")]
            public string ProjectName { get; set; } = "error-crawler-csharp";
        }

}

