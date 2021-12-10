
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace error_reporting_csharp_dotnet_tool
{
    public class ErrorCodeCollection
    {
        private Dictionary<string, ErrorCodeEntry> errorCodeDictionary;
        public string ProjectShortTag { get; set; }

        public string ProjectName { get; set; }
        public string ProjectVersion { get; set; }
        public ErrorCodeCollection()
        {
            errorCodeDictionary = new Dictionary<string, ErrorCodeEntry>();
        }

        public ErrorCodeEntry AddEntry(String identifier)
        {
            if (errorCodeDictionary.ContainsKey(identifier))
            {
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
        //https://www.newtonsoft.com/json/help/html/readingwritingjson.htm
        public void BuildJSON()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                writer.WritePropertyName("$schema");
                writer.WriteValue("https://schemas.exasol.com/error_code_report-0.2.0.json");

                writer.WritePropertyName("projectName");
                writer.WriteValue(ProjectName);

                writer.WritePropertyName("projectVersion");
                writer.WriteValue(ProjectVersion);

                writer.WritePropertyName("errorCodes");
                writer.WriteStartArray();
                //start of error codes array
                WriteErrorCodeEntries(writer);
                //end of error codes array
                writer.WriteEnd();

                writer.WriteEndObject();

            }
        }

        private void WriteErrorCodeEntries(JsonWriter writer)
        {
            foreach (var errorCodeEntry in errorCodeDictionary)
            {
                WriteErrorCodeEntry(writer, errorCodeEntry);
            }
        }

        private static void WriteErrorCodeEntry(JsonWriter writer, KeyValuePair<string, ErrorCodeEntry> errorCodeEntry)
        {
            writer.WriteValue("DVD read/writer");
            writer.WriteComment("(broken)");

            writer.WriteValue("500 gigabyte hard drive");
            writer.WriteValue("200 gigabyte hard drive");
        }
    }
}

