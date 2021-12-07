using Exasol.ErrorReporting;
using System;

namespace error_reporting_dotnet_tool_testproject
{
    /// <summary>
    /// This is the project
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Don't pick this up");

            Console.WriteLine("Something went wrong:" + ExaError.MessageBuilder("E-ERJ-TEST-1").ToString());

            var exaErrorStr1 = ExaError.MessageBuilder("E-ERJ-TEST-1").ToString();

            var exaErrorObject1 = ExaError.MessageBuilder("E-ERJ-TEST-1");
            exaErrorObject1.Message("Woops! Something went wrong!");





        }
    }
}
