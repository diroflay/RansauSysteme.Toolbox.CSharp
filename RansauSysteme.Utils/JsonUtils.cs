using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RansauSysteme.Utils
{
    /// <summary>
    /// Provides utility methods for JSON serialization and deserialization.
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// Gets the default JsonSerializerOptions with proper formatting and enum conversion.
        /// </summary>
        public static JsonSerializerOptions DefaultSerializerOptions => new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="serializerOptions">Custom serializer options, or null to use the default options.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(object objectToSerialize, JsonSerializerOptions? serializerOptions = null)
        {
            if (objectToSerialize == null)
                throw new ArgumentNullException(nameof(objectToSerialize));

            var options = serializerOptions ?? DefaultSerializerOptions;
            return JsonSerializer.Serialize(objectToSerialize, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <param name="serializerOptions">Custom serializer options, or null to use the default options.</param>
        /// <returns>The deserialized object, or default(T) if the JSON string is null, empty, or "{}".</returns>
        public static T? ParseJson<T>(string? jsonString, JsonSerializerOptions? serializerOptions = null)
        {
            if (string.IsNullOrEmpty(jsonString) || jsonString == "{}")
            {
                return default;
            }

            var options = serializerOptions ?? DefaultSerializerOptions;
            return JsonSerializer.Deserialize<T>(jsonString, options);
        }

        /// <summary>
        /// Reads a JSON file and deserializes its contents to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="filePath">The path to the JSON file.</param>
        /// <param name="serializerOptions">Custom serializer options, or null to use the default options.</param>
        /// <returns>The deserialized object, or default(T) if the file doesn't exist or is empty.</returns>
        public static T? ParseFile<T>(string filePath, JsonSerializerOptions? serializerOptions = null)
        {
            if (!File.Exists(filePath))
            {
                return default;
            }

            var content = File.ReadAllText(filePath);

            return ParseJson<T>(content, serializerOptions);
        }

        /// <summary>
        /// Saves an object as JSON to a file.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize and save.</param>
        /// <param name="filePath">The path where the JSON file should be saved.</param>
        /// <param name="serializerOptions">Custom serializer options, or null to use the default options.</param>
        public static void SaveToFile(object objectToSerialize, string filePath, JsonSerializerOptions? serializerOptions = null)
        {
            string json = ToJson(objectToSerialize, serializerOptions);

            // Create directory if it doesn't exist
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
        }
    }
}