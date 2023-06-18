namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MissingMod
    {
        public MissingMod(string steamId)
        {
            SteamId = steamId;
        }

        public string SteamId { get; }

        public string SteamUri => $"steam://url/CommunityFilePage/{SteamId}";
    }
}