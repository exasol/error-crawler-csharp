using CommandLine;

using System.Threading.Tasks;

namespace error_reporting_csharp_dotnet_tool
{
    //https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create
    partial class Program
    {
        static async Task Main(string[] args)
        {
            //https://github.com/commandlineparser/commandline
            await Parser.Default.ParseArguments<Options>(args)
         .WithParsedAsync<Options>(ExtractErrorCatalogInformation.RunAsync); ;
        }

    }
}

