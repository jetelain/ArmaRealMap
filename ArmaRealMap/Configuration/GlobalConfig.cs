namespace ArmaRealMap
{
    public class GlobalConfig
    {
        public string CacheLocationBase { get; set; }

        public string LibrariesFile { get; set; }
        public string ModelsInfoFile { get; set; }
        public string TerrainMaterialFile { get; set; }
        public string RoadTypesFile { get; set; }

        public SRTMConfig SRTM { get; set; }

        public S2CConfig S2C { get; set; }
        public bool SyncWithAssetManager { get; set; } = true;
        public string AssetManager { get; set; } = "https://arm.pmad.net/";
    }
}
