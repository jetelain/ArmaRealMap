using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaRealMap.Configuration
{
    internal class ConfigLoader
    {
        internal static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() },
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        internal static GlobalConfig LoadGlobal(string globalConfigFile)
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArmaRealMap");
            if (string.IsNullOrEmpty(globalConfigFile))
            {
                globalConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArmaRealMap", "config.json");
            }
            GlobalConfig config;
            if (File.Exists(globalConfigFile))
            {
                Console.WriteLine($"Read global config from '{globalConfigFile}':");
                config = ReadConfigFile<GlobalConfig>(globalConfigFile);
            }
            else
            {
                Console.WriteLine($"Global config not found at '{globalConfigFile}', use defaults:");
                config = new GlobalConfig();
            }
            config.S2C = config.S2C ?? new S2CConfig();
            config.SRTM = config.SRTM ?? new SRTMConfig();
            config.S2C.CacheLocation   = GetExistingPath(globalConfigFile, config.S2C.CacheLocation ?? Path.Combine(config.CacheLocationBase ?? basePath, "S2C"));
            config.SRTM.CacheLocation  = GetExistingPath(globalConfigFile, config.SRTM.CacheLocation ?? Path.Combine(config.CacheLocationBase ?? basePath, "SRTM"));
            config.LibrariesFile       = GetPath(globalConfigFile, config.LibrariesFile ?? Path.Combine(config.CacheLocationBase ?? basePath, "libraries.json"));
            config.ModelsInfoFile      = GetPath(globalConfigFile, config.ModelsInfoFile ?? Path.Combine(config.CacheLocationBase ?? basePath, "models.json"));
            config.TerrainMaterialFile = GetPath(globalConfigFile, config.TerrainMaterialFile ?? Path.Combine(config.CacheLocationBase ?? basePath, "terrain.json"));
            Console.WriteLine($"S2C.CacheLocation   = '{config.S2C.CacheLocation}'");
            Console.WriteLine($"SRTM.CacheLocation  = '{config.SRTM.CacheLocation}'");
            Console.WriteLine($"LibrariesFile       = '{config.LibrariesFile}'");
            Console.WriteLine($"ModelsInfoFile      = '{config.ModelsInfoFile}'");
            Console.WriteLine($"TerrainMaterialFile = '{config.TerrainMaterialFile}'");
            Console.WriteLine();
            return config;
        }

        private static T ReadConfigFile<T>(string filename)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(filename), SerializerOptions);
        }

        internal static MapConfig LoadConfig(string configFilePath)
        {
            configFilePath = Path.GetFullPath(configFilePath);

            Console.WriteLine($"Read map config from '{configFilePath}':");

            var config = JsonSerializer.Deserialize<MapConfig>(File.ReadAllText(configFilePath), new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() },
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            if (string.IsNullOrEmpty(config.WorldName))
            {
                config.WorldName = Path.GetFileNameWithoutExtension(configFilePath).ToLowerInvariant();
            }

            if (string.IsNullOrEmpty(config.PboPrefix))
            {
                config.PboPrefix = "z\\arm\\addons\\" + config.WorldName;
            }

            var basePath = Path.Combine(Path.GetDirectoryName(configFilePath), config.WorldName);

            if (config.Target == null)
            {
                config.Target = new TargetConfig();
            }

            config.Target.Debug         = GetExistingPath(configFilePath, config.Target.Debug         ?? Path.Combine(basePath, "debug"));
            config.Target.Terrain       = GetExistingPath(configFilePath, config.Target.Terrain       ?? Path.Combine(basePath, "output", "terrain"));
            config.Target.Cooked        = GetExistingPath(configFilePath, config.Target.Cooked        ?? Path.Combine(basePath, "output", "precooked"));
            config.Target.InternalCache = GetExistingPath(configFilePath, config.Target.InternalCache ?? Path.Combine(basePath, "cache", "internal"));
            config.Target.InputCache    = GetExistingPath(configFilePath, config.Target.InputCache    ?? Path.Combine(basePath, "cache", "input"));

            Console.WriteLine($"Target.Debug         = '{config.Target.Debug}'");
            Console.WriteLine($"Target.Terrain       = '{config.Target.Terrain}'");
            Console.WriteLine($"Target.Cooked        = '{config.Target.Cooked}'");
            Console.WriteLine($"Target.InternalCache = '{config.Target.InternalCache}'");
            Console.WriteLine($"Target.InputCache    = '{config.Target.InputCache}'");
            Console.WriteLine();

            Directory.CreateDirectory(Path.Combine(config.Target.Objects));
            Directory.CreateDirectory(Path.Combine(config.Target.Terrain, "idmap"));
            Directory.CreateDirectory(Path.Combine(config.Target.Terrain, "sat"));
            return config;
        }

        private static string GetPath(string configFilePath, string value)
        {
            return Path.Combine(Path.GetDirectoryName(configFilePath), Environment.ExpandEnvironmentVariables(value));
        }

        private static string GetExistingPath(string configFilePath, string value)
        {
            var fullPath = GetPath(configFilePath, value);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        public static void UpdateFile(string target, string source)
        {
            var client = new HttpClient();
            Trace.TraceInformation($"Download '{source}' to '{target}'");
            try
            {
                File.WriteAllText(target, client.GetStringAsync(source).ConfigureAwait(false).GetAwaiter().GetResult());
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unable to download '{source}' to '{target}': {ex.Message}");
                Trace.TraceError($"Failed: {ex}");
                if (!File.Exists(target))
                {
                    throw new ApplicationException($"No previously downloaded file '{target}' exists. Unable to proceed.");
                }
            }
        }

    }
}
