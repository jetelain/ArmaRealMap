using System.Collections.Generic;

namespace TerrainBuilderUtil
{
    internal class ModInfo
    {
        public string Path { get; internal set; }
        public List<PboInfo> Pbos { get; internal set; } = new List<PboInfo>();
        public string WorkshopId { get; internal set; }
    }
}