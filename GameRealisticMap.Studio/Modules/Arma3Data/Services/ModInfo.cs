namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal class ModInfo
    {
        public ModInfo(string name, string path, string? steamId = null, string? prefix = null, string? downloadUri = null)
        {
            Name = name;
            Path = path;
            SteamId = steamId;
            Prefix = prefix;
            DownloadUri = downloadUri;
            IsCdlc = !string.IsNullOrEmpty(downloadUri);
        }

        public string Name { get; }

        public string Path { get; }

        public string? SteamId { get; }

        public string? Prefix { get; }

        public string? DownloadUri { get; }

        public bool IsCdlc { get; }
    }
}
