using error_reporting_csharp_dotnet_tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace error_reporting_csharp_dotnet_tool_tests
{
    public class Tests
    {

        [Fact]
        public async void IntegrationTest()
        {
            File.Delete("error_code_report.json");
            PackageAndInstallTool();
            RunErrorCrawlerOnTestProject();
            CheckResultingJSON();
        }

        private void PackageAndInstallTool()
        {
            RunCmdStatement("cd ../../../.. && dotnet pack && dotnet tool update -g --add-source .\\error-reporting-csharp-dotnet-tool\\nupkg\\ exasol-error-crawler");
        }
    

        private static void CheckResultingJSON()
        {
            var jsonStr = File.ReadAllText("error_code_report.json");
            var compareStr = "{\r\n  \"$schema\": \"https://schemas.exasol.com/error_code_report-1.0.0.json\",\r\n  \"projectName\": \"error-crawler-csharp\",\r\n  \"errorCodes\": [\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-1\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-2\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-3\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4bis\",\r\n      \"message\": \"Woops! Something went wrong! 1Oh my! Something went wrong! 2\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-1\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-2\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\",\r\n        \"Don't just sit there 2\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTSI-1\",\r\n      \"message\": \"Woops! {somethingWentWrongValue}\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTCONCAT-1\",\r\n      \"message\": \"Woops! + Woops!\",\r\n      \"mitigations\": []\r\n    }\r\n  ]\r\n}";
            Assert.True(jsonStr == compareStr);
        }

        private static void RunErrorCrawlerOnTestProject()
        {
            string projectEntry = "..\\..\\..\\..\\error-reporting-dotnet-tool-testproject\\error-reporting-dotnet-tool-testproject.csproj";
            string argument = $"exasol-error-crawler -t ECC -p {projectEntry}";
            RunCmdStatement(argument);
        }

        private static void RunCmdStatement(string argument)
        {
            //https://stackoverflow.com/questions/1469764/run-command-prompt-commands
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {argument}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

        }
    }
}
