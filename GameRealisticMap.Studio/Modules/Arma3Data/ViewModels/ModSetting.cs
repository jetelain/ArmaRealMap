using GameRealisticMap.Studio.Modules.Arma3Data.Services;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    internal class ModSetting
    {
        public ModSetting(bool isActive, ModInfo modInfo)
        {
            IsActive = isActive;
            ModInfo = modInfo;
        }

        public bool IsActive { get; set; }

        public string Name => ModInfo.Name;

        public string Path => ModInfo.Path;

        public ModInfo ModInfo { get; }
    }
}