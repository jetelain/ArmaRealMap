using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.Assets
{
    public class ModDependencyDefinition
    {
        [JsonConstructor]
        public ModDependencyDefinition(string steamId)
        {
            SteamId = steamId;
        }

        public string SteamId { get; }
    }
}