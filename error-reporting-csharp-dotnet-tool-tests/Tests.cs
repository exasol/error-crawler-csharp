using error_reporting_csharp_dotnet_tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace error_reporting_csharp_dotnet_tool_tests
{
    public class Tests
    {
        [Fact]
        public async void TestAllProjects()
        {
            Options o = new Options();
            o.ProjectShortTag = "ECC";


            await ExtractErrorCatalogInformation.RunAsync(o);
            //Assert.True(output == expectedOutput);
        }

        [Fact]
        public async void TestOneProjectPath()
        {
            Options o = new Options();
            o.ProjectShortTag = "ECC";

            await ExtractErrorCatalogInformation.RunAsync(o);

        }
        [Fact]
        public async void TestWrongShorttag()
        {
            Options o = new Options();
            o.ProjectShortTag = "EOC";

            await ExtractErrorCatalogInformation.RunAsync(o);

        }
        //test duplicates
    }
}
