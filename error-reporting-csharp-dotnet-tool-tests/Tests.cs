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
        public async void TestOneProjectPath()
        {
            string[] projectEntries = SetProjectEntries();

            ErrorCodeCollection errorCodeCollection = new ErrorCodeCollection();
            errorCodeCollection.ProjectShortTag = "ECC";
            errorCodeCollection.ProjectName = "test-project-name";

            await ExtractErrorCatalogInformation.CollectErrorCodes(projectEntries, errorCodeCollection);

            Assert.True(errorCodeCollection.Count > 5);
            //string generatedJSON = errorCodeCollection.GenerateJSON();

        }

        private string[] SetProjectEntries()
        {
            string[] projectEntries = new string[1];
            projectEntries[0] = "..\\..\\..\\..\\error-reporting-dotnet-tool-testproject\\error-reporting-dotnet-tool-testproject.csproj";
            return projectEntries;
        }

        //[Fact]
        //public async void TestWrongShorttag()
        //{
        
        //}
        ////test duplicates
    }
}
