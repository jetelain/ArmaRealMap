using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Edit
{
    public class SimpleWrpModConfig : IPboConfig
    {
        public SimpleWrpModConfig(string worldName, string pboPrefix, string? targetModDirectory = null) 
        {
            WorldName = worldName;
            PboPrefix = pboPrefix;
            TargetModDirectory = string.IsNullOrEmpty(targetModDirectory) ? Arma3MapConfig.GetAutomaticTargetModDirectory(worldName) : targetModDirectory;
        }

        public string PboPrefix { get; }

        public string TargetModDirectory { get; }

        public string WorldName { get; }
    }
}