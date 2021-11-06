using System.Collections.Generic;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public class JsonObjectLibrary
    {
        public ObjectCategory Category { get; set; }

        public List<SingleObjet> Objects { get; set; }

        public TerrainRegion? Terrain { get; set; }

        public double? Density { get; set; }
    }
}
