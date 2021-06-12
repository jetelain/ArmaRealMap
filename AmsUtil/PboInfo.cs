using System;
using System.Collections.Generic;

namespace TerrainBuilderUtil
{
    internal class PboInfo
    {
        public string Path { get; internal set; }
        public HashSet<string> Files { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public ModInfo Mod { get; internal set; }
    }
}