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
            exaErrorObject1.Message("Woops! Something went wrong! 1");
            //Case 4
            var exaErrorObject2 = ExaError.MessageBuilder("E-ECC-TEST-4").Message("Woops! Something went wrong! 1");
            var exaErrorObject3 = ExaError.MessageBuilder("E-ECC-TEST-4bis").Message("Woops! Something went wrong! 1").Message("Oh my! Something went wrong! 2"); ;

            //
            var exaErrorObjectMitigation1 = ExaError.MessageBuilder("E-ECC-TESTMIT-1").Message("Woops! Something went wrong!").Mitigation("Do something about it 1");
            var exaErrorObjectMitigation2 = ExaError.MessageBuilder("E-ECC-TESTMIT-2").Message("Woops! Something went wrong!").Mitigation("Do something about it 1").Mitigation("Don't just sit there 2");

            string somethingWentWrongValue = "Something went very wrong";
            ExaError.MessageBuilder("E-ECC-TESTSI-1").Message($@"Woops! {somethingWentWrongValue}");
            ExaError.MessageBuilder("E-ECC-TESTCONCAT-1").Message($@"Woops!" + "Woops!");
            //TODO: maybe later, allow for this, add more robustness, check for context, identifiers ..
            ////mixed cases:
            //var mixed1 = ExaError.MessageBuilder("E-ECC-MIXED-1");
            //var mixed2 = ExaError.MessageBuilder("E-ECC-MIXED-2");

            //mixed1.Message("Add something afterwards 1");
            //mixed2.Message("Add something afterwards 2");
        }
    }
}
