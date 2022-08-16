using System.Text.Json;

namespace Shibusa.DevTools.AppServices
{
    public class ConfigurationService
    {
        public static class Keys
        {
            public const string FindLines = "fl";
            public const string CsProjects = "cs";
            public const string Sql = "sql";
            public const string Config = "config";

            public static IEnumerable<string> GetAll()
            {
                foreach (var fi in typeof(Keys).GetFields())
                {
                    var key = fi.GetValue(null)?.ToString();
                    if (key != null)
                    {
                        yield return key;
                    }
                }
            }
        }

        private static JsonSerializerOptions serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task<IDictionary<string, Dictionary<string, string>>> GetConfigurationAsync(FileInfo fileInfo)
        {
            if (!fileInfo.Exists) { return await CreateConfigurationFileAsync(fileInfo); }

            var dictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                File.ReadAllText(fileInfo.FullName), serializerOptions);

            if (dictionary != null && dictionary.Any())
            {
                UpdateDictionary(dictionary);
                return dictionary;
            }

            return await CreateConfigurationFileAsync(fileInfo);

            throw new Exception($"Unable to deserialize configuration file: {fileInfo.FullName}");
        }

        public static async Task SaveConfigurationFileAsync(IDictionary<string, Dictionary<string, string>> configDictionary, FileInfo configFileInfo)
        {
            UpdateDictionary(configDictionary);
            var json = JsonSerializer.Serialize(configDictionary, serializerOptions);
            if (json == null) { throw new ArgumentException($"{nameof(configDictionary)} could not be serialized."); }
            await File.WriteAllTextAsync(configFileInfo.FullName, json);
        }

        private static void UpdateDictionary(IDictionary<string, Dictionary<string, string>> dictionary)
        {
            foreach (var key in Keys.GetAll().ToArray())
            {
                if (!dictionary.ContainsKey(key))
                {
                    dictionary[key] = new Dictionary<string, string>();
                }
            }
        }

        private static async Task<IDictionary<string, Dictionary<string, string>>> CreateConfigurationFileAsync(FileInfo fileInfo)
        {
            Dictionary<string, Dictionary<string, string>> config = new();

            foreach (var key in Keys.GetAll().ToArray())
            {
                config.Add(key, new Dictionary<string, string>());
            }

            await SaveConfigurationFileAsync(config, fileInfo);

            return config;
        }
    }
}
