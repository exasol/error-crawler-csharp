using error_reporting_csharp_dotnet_tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace error_reporting_csharp_dotnet_tool_tests
{
    public class Tests
    {


        [Fact]
        public void IntegrationTest()
        {
            File.Delete("error_code_report.json");
            PackageAndInstallTool();
            BuildTestProject();
            RunErrorCrawlerOnTestProject();
            CheckResultingJSON();
        }
        [Fact]
        public void IntegrationTestNoProjectTag()
        {
            File.Delete("error_code_report.json");
            PackageAndInstallTool();
            BuildTestProject();
            RunErrorCrawlerOnTestProjectWithoutProjectName();
            CheckResultingJSONWithoutProjectName();
        }
        private static void PackageAndInstallTool()
        {
            string output = RunCmd("cd ../../../.. && dotnet pack");

            string buildVersion = ExtractBuildVersionFromPackageOutput(output);
            RunCmd($"cd ../../../.. && dotnet tool update -g --add-source .\\error-reporting-csharp-dotnet-tool\\nupkg\\ exasol-error-crawler --version {buildVersion}");
        }

        private static string ExtractBuildVersionFromPackageOutput(string output)
        {
            string buildVersion = string.Empty;
            string packagePathRegexStr = "(?<= Successfully created package ')[\\w\\:\\\\\\-\\.]*(?=')";
            Regex packagePathRegex = new Regex(packagePathRegexStr);
            var packagePathMatch = packagePathRegex.Match(output);
            if (packagePathMatch.Success)
            {
                string versionRegexStr = "(?<=crawler\\.)[\\w\\W]*(?=.nupkg)";
                Regex versionRegex = new Regex(versionRegexStr);
                var versionMatch = versionRegex.Match(packagePathMatch.Value);
                if (versionMatch.Success)
                {
                    //https://stackoverflow.com/questions/19774155/returning-a-string-from-a-console-application
                    buildVersion = versionMatch.Value;
                }
            }

            return buildVersion;
        }

        private static void CheckResultingJSON()
        {
            var jsonStr = File.ReadAllText("error_code_report.json");
            var compareStr = "{\r\n  \"$schema\": \"https://schemas.exasol.com/error_code_report-1.0.0.json\",\r\n  \"projectName\": \"error-crawler-csharp\",\r\n  \"errorCodes\": [\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-1\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-2\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-3\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4bis\",\r\n      \"message\": \"Woops! Something went wrong! 1Oh my! Something went wrong! 2\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-1\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-2\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\",\r\n        \"Don't just sit there 2\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTSI-1\",\r\n      \"message\": \"Woops! {{somethingWentWrongValue}}\",\r\n      \"messagePlaceholders\": [\r\n        {\r\n          \"placeholder\": \"somethingWentWrongValue\"\r\n        }\r\n      ],\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTCONCAT-1\",\r\n      \"message\": \"Woops! + Woops!\",\r\n      \"mitigations\": []\r\n    }\r\n  ]\r\n}";
            Assert.True(jsonStr == compareStr);
        }
        private static void BuildTestProject()
        {
            string argument = "cd ..\\..\\..\\..\\error-reporting-dotnet-tool-testproject\\ && dotnet build";
            RunCmd(argument);
        }
        private static void RunErrorCrawlerOnTestProject()
        {
            string projectEntry = "..\\..\\..\\..\\error-reporting-dotnet-tool-testproject\\error-reporting-dotnet-tool-testproject.csproj";
            string argument = $"exasol-error-crawler -t ECC -p {projectEntry} -n error-crawler-csharp";
            RunCmd(argument);
        }
        private static void RunErrorCrawlerOnTestProjectWithoutProjectName()
        {
            string projectEntry = "..\\..\\..\\..\\error-reporting-dotnet-tool-testproject\\error-reporting-dotnet-tool-testproject.csproj";
            string argument = $"exasol-error-crawler -t ECC -p {projectEntry}";
            RunCmd(argument);
        }
        private static void CheckResultingJSONWithoutProjectName()
        {
            var jsonStr = File.ReadAllText("error_code_report.json");
            var compareStr = "{\r\n  \"$schema\": \"https://schemas.exasol.com/error_code_report-1.0.0.json\",\r\n  \"errorCodes\": [\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-1\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-2\",\r\n      \"message\": \"\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-3\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4\",\r\n      \"message\": \"Woops! Something went wrong! 1\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TEST-4bis\",\r\n      \"message\": \"Woops! Something went wrong! 1Oh my! Something went wrong! 2\",\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-1\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTMIT-2\",\r\n      \"message\": \"Woops! Something went wrong!\",\r\n      \"mitigations\": [\r\n        \"Do something about it 1\",\r\n        \"Don't just sit there 2\"\r\n      ]\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTSI-1\",\r\n      \"message\": \"Woops! {{somethingWentWrongValue}}\",\r\n      \"messagePlaceholders\": [\r\n        {\r\n          \"placeholder\": \"somethingWentWrongValue\"\r\n        }\r\n      ],\r\n      \"mitigations\": []\r\n    },\r\n    {\r\n      \"identifier\": \"E-ECC-TESTCONCAT-1\",\r\n      \"message\": \"Woops! + Woops!\",\r\n      \"mitigations\": []\r\n    }\r\n  ]\r\n}";
            Assert.True(jsonStr == compareStr);
        }
        //https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
        private static string RunCmd(string argument)
        {
            ConsoleOutput co = new ConsoleOutput();
            //* Create your Process
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {argument}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += new DataReceivedEventHandler(co.OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(co.ErrorHandler);
            //* Start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            if (co.Errors.Length > 0)
            {
                throw new Exception($"Something went wrong executing the command: {Environment.NewLine + co.Errors}");
            }
            return co.Output;
        }

    }
    class ConsoleOutput
    {
        public string Output { get; set; }
        public string Errors { get; set; }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Output += (outLine.Data);
        }
        public void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Errors += outLine.Data;
        }
    }
}
