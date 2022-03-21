
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
                if (ProjectName != null)
                {
                    writer.WritePropertyName("projectName");
                    writer.WriteValue(ProjectName);
                }
                WriteErrorCodeSection(writer);

                writer.WriteEndObject();

            }

            return sb.ToString();
        }

        private static void ValidateGeneratedJSON(string generatedJson)
        {
            var schema = JsonSchema.FromJsonAsync(JSONSchemaStore.GetJSONSchema()).Result;
            var validationErrors = schema.Validate(generatedJson);
            if (validationErrors.Count > 0)
            {
                string validationSchemaErrors = string.Empty;
                foreach (var validationError in validationErrors)
                {
                    validationSchemaErrors += Environment.NewLine + validationError;
                }
                
                throw new Exception("JSON Schema validation failed:" + validationSchemaErrors);
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
            List<String> placeholderValues = new List<String>();
            messageStr = RewriteStringInterpolationAndAddPlaceholders(messageStr, placeholderValues);
            writer.WriteValue(messageStr);

            WritePlaceholders(writer, placeholderValues);
        }

        private static void WritePlaceholders(JsonWriter writer, List<string> placeholderValues)
        {
            if (placeholderValues.Count > 0) { 
            writer.WritePropertyName("messagePlaceholders");
            writer.WriteStartArray();
            foreach (var placeholder in placeholderValues)
            {
                WritePlaceholder(writer, placeholder);
            }
            writer.WriteEndArray();
            }
        }

        private static void WritePlaceholder(JsonWriter writer, string placeholder)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("placeholder");
            writer.WriteValue(placeholder);
            writer.WriteEndObject();
        }

        private static string RewriteStringInterpolationAndAddPlaceholders(string messageStr, List<String> placeholderValues)
        {          
            //{[\w\W]*}
            Regex stringInterpolation = new Regex("{[\\w\\.^}]*}");
            return stringInterpolation.Replace(messageStr, match => RewriteWithExtraBrackets(match, placeholderValues));           
        }

        private static string RewriteWithExtraBrackets(Match match, List<string> placeHolderValues)
        {
            placeHolderValues.Add(match.Value.Substring(1,match.Value.Length-2).Replace("{","").Replace("}",""));
            return $"{{{match.Value}}}";
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

