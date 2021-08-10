using System.Collections.Generic;
using System.Linq;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibrary
    {
        public ObjectCategory Category { get; set; }

        public List<SingleObjetInfos> Objects { get; set; }

        public List<CompositionInfos> Compositions { get; set; }

        public TerrainRegion? Terrain { get; set; }

        public double? Density { get; set; }

        internal SingleObjetInfos GetObject(string name)
        {
            return Objects.First(o => o.Name == name);
        }
    }
}
