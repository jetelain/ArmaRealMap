namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    internal class ModSetting
    {
        public ModSetting(bool isActive, string name, string path)
        {
            IsActive = isActive;
            Name = name;
            Path = path;
        }

        public bool IsActive { get; set; }

        public string Name { get; }    

        public string Path { get; }
    }
}