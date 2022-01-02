using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Libraries
{
    public class CompositionInfos : ObjetInfosBase
    {
        public List<CompositionObjetInfos> Objects { get; set; }

        internal override IEnumerable<TerrainObject> ToObjects(IBoundingShape box)
        {
            return Objects.Select(o => o.ToObject(box));
        }
    }
}