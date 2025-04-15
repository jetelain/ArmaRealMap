using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.Assets
{
    [DebuggerDisplay("{SteamId} ({CdlcPath})")]
    public class ModDependencyDefinition
    {
        [JsonConstructor]
        public ModDependencyDefinition(string steamId, string? cdlcPath = null)
        {
            SteamId = steamId;
            CdlcPath = cdlcPath;
        }

        public string SteamId { get; }

        public string? CdlcPath { get; }

        [JsonIgnore]
        public bool IsCdlc => !string.IsNullOrEmpty(CdlcPath);
    }
}