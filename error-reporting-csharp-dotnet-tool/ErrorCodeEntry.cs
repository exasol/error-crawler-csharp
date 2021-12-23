using System.Collections.Generic;

namespace error_reporting_csharp_dotnet_tool
{
    public class ErrorCodeEntry
    {
        public ErrorCodeEntry(string identifier)
        {
            Identifier = identifier;
            Messages = new List<string>();
            Mitigations = new List<string>();
        }

        public string Identifier { get; set; }
        public List<string> Messages { get; set; }
        public List<string> Mitigations { get; set; }

    }
}

