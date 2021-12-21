
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
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
        public int Count { get { return errorCodeDictionary.Count; }}
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
        //https://github.com/RicoSuter/NJsonSchema/wiki/JsonSchemaValidator
        public string GenerateJSON()
        {
            return BuildAndValidateJSON();
        }

        private string BuildAndValidateJSON()
        {
            var generatedJSON = BuildJSON();
            ValidateGeneratedJSON(generatedJSON);
            return generatedJSON;
        }

        private string BuildJSON()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                writer.WritePropertyName("$schema");
                writer.WriteValue("https://schemas.exasol.com/error_code_report-1.0.0.json");

                writer.WritePropertyName("projectName");
                writer.WriteValue(ProjectName);

                WriteErrorCodeSection(writer);

                writer.WriteEndObject();

            }

            return sb.ToString();
        }

        private static void ValidateGeneratedJSON(string generatedJson)
        {
            //var schema = JsonSchema.FromFileAsync(@"schema\error_code_report-1.0.0.json").Result;
            var schema = JsonSchema.FromJsonAsync(JSONSchemaStore.GetJSONSchema()).Result;
            var result = schema.Validate(generatedJson);
            if (result.Count > 0)
            {
                throw new Exception("JSON Schema validation failed.");
            }
        }

        private void WriteErrorCodeSection(JsonWriter writer)
        {
            writer.WritePropertyName("errorCodes");
            WriteErrorCodeEntries(writer);
        }

        private void WriteErrorCodeEntries(JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var errorCodeEntry in errorCodeDictionary)
            {
                WriteErrorCodeEntry(writer, errorCodeEntry);
            }
            writer.WriteEnd();
        }

        private static void WriteErrorCodeEntry(JsonWriter writer, KeyValuePair<string, ErrorCodeEntry> errorCodeEntry)
        {
            var errorCodeEntryValue = errorCodeEntry.Value;

            writer.WriteStartObject();

            WriteIdentifier(writer, errorCodeEntryValue);

            WriteMessage(writer, errorCodeEntryValue);

            WriteMitigations(writer, errorCodeEntryValue);

            writer.WriteEndObject();
        }

        private static void WriteIdentifier(JsonWriter writer, ErrorCodeEntry errorCodeEntryValue)
        {
            writer.WritePropertyName("identifier");
            writer.WriteValue(errorCodeEntryValue.Identifier);
        }

        private static void WriteMessage(JsonWriter writer, ErrorCodeEntry errorCodeEntryValue)
        {
            writer.WritePropertyName("message");
            string messageStr = string.Empty;
            foreach (var message in errorCodeEntryValue.Messages)
            {
                messageStr += message;
            }
            writer.WriteValue(messageStr);
        }

        private static void WriteMitigations(JsonWriter writer, ErrorCodeEntry errorCodeEntryValue)
        {
            writer.WritePropertyName("mitigations");
            writer.WriteStartArray();
            foreach (var mitigation in errorCodeEntryValue.Mitigations)
            {
                writer.WriteValue(mitigation);
            }
            writer.WriteEnd();
        }
    }
}

