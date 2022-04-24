namespace ArmaRealMap
{
    public class GlobalConfig
    {
        public string CacheLocationBase { get; set; }

        public string LibrariesFile { get; set; }
        public string ModelsInfoFile { get; set; }

        public SRTMConfig SRTM { get; set; }

        public S2CConfig S2C { get; set; }
    }
}
