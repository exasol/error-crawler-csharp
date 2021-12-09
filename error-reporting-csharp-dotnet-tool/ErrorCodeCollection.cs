using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace error_reporting_csharp_dotnet_tool
{
    public class ErrorCodeCollection
    {
        private Dictionary<string, ErrorCodeEntry> errorCodeDictionary;
        public string ProjectShortTag { get; set; }
        public ErrorCodeCollection()
        {
            errorCodeDictionary = new Dictionary<string, ErrorCodeEntry>();
        }

        public ErrorCodeEntry AddEntry(String identifier)
        {
            if (errorCodeDictionary.ContainsKey(identifier)){
                throw new Exception($@"Identifier {identifier} previously used in project(s). Please correct this.");
            }
            if (!identifier.Contains(ProjectShortTag))
            {
                throw new Exception($@"Make sure you're using the right short tag for this project: {ProjectShortTag} , the conflicting identifier is: {identifier} .");
            }

            ErrorCodeEntry newECEntry = new ErrorCodeEntry(identifier);
            errorCodeDictionary.Add(identifier, newECEntry);
            return newECEntry;
        }
    }
}

