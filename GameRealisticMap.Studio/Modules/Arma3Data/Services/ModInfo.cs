namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal class ModInfo
    {
        public ModInfo(string name, string path, string? steamId = null)
        {
            Name = name;
            Path = path;
            SteamId = steamId;
        }

        public string Name { get; }

        public string Path { get; }

        public string? SteamId { get; }
    }
}
