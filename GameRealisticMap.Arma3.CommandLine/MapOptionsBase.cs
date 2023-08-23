using System.Text.Json;
using CommandLine;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class MapOptionsBase
    {
        [Option('c', "config", Required = true, HelpText = "Map configuration file (.grma3m file)")]
        public string ConfigFile { get; set; } = string.Empty;


        public async Task<Arma3MapConfig> GetConfigFile()
        {
            Arma3MapConfig a3config;
            using (var stream = File.OpenRead(ConfigFile))
            {
                var config = await JsonSerializer.DeserializeAsync<Arma3MapConfigJson>(stream) ?? new Arma3MapConfigJson();
                a3config = config.ToArma3MapConfig();
            }
            return a3config;
        }

        public async Task<MapWorkspace> CreateWorkspace()
        {
            return await MapWorkspace.Create(await GetConfigFile(), Path.GetDirectoryName(ConfigFile) ?? string.Empty);
        }
    }
}
