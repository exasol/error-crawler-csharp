using Exasol.ErrorReporting;
using System;

namespace error_reporting_dotnet_tool_testproject
{
    /// <summary>
    /// This is a test project
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Don't pick this up");
            //Case 1
            Console.WriteLine("Something went wrong:" + ExaError.MessageBuilder("E-ECC-TEST-1").ToString());
            //Case 2
            var exaErrorString = ExaError.MessageBuilder("E-ECC-TEST-2").ToString();
            //Case 3
            var exaErrorObject1 = ExaError.MessageBuilder("E-ECC-TEST-3");
            exaErrorObject1.Message("Woops! Something went wrong!");
            //Case 4
            var exaErrorObject2 = ExaError.MessageBuilder("E-ECC-TEST-4").Message("Woops! Something went wrong!");

            //
            var exaErrorObjectMitigation1 = ExaError.MessageBuilder("E-ECC-TEST-5").Message("Woops! Something went wrong!").Mitigation("Do something about it");
            var exaErrorObjectMitigation2 = ExaError.MessageBuilder("E-ECC-TEST-5").Message("Woops! Something went wrong!").Mitigation("Do something about it").Mitigation("Don't just sit there");


        }
    }
}
