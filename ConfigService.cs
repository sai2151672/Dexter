using System;
using System.IO;
using System.Text.Json;

namespace Dexter
{
    public static class ConfigService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static string GetConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dexter-config.json");
        }

        public static DexterConfig LoadConfig()
        {
            string configPath = GetConfigPath();

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Config file not found: {configPath}");
            }

            string json = File.ReadAllText(configPath);

            var config = JsonSerializer.Deserialize<DexterConfig>(json, JsonOptions);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize dexter-config.json.");
            }

            return config;
        }

        public static string LoadConfigText()
        {
            string configPath = GetConfigPath();

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Config file not found: {configPath}");
            }

            return File.ReadAllText(configPath);
        }

        public static DexterConfig ParseConfig(string json)
        {
            var config = JsonSerializer.Deserialize<DexterConfig>(json, JsonOptions);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize dexter-config.json.");
            }

            return config;
        }

        public static void SaveConfig(DexterConfig config)
        {
            string configPath = GetConfigPath();
            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(configPath, json);
        }
    }
}