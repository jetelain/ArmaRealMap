using System.Collections.Generic;
using System.Numerics;
using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    internal class TerrainSpacialIndex<T> : SimpleSpacialIndex<T> where T : class, ITerrainGeometry
    {
        public TerrainSpacialIndex(MapInfos map, int cellCount = 512)
            : base(map.P1.Vector, new Vector2(map.Width, map.Height), cellCount)
        {
        }

        public void Insert(T obj)
        {
            Insert(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public List<T> Search(TerrainPoint start, TerrainPoint end)
        {
            return Search(start.Vector, end.Vector);
        }
        public List<T> Search(ITerrainGeometry obj)
        {
            return Search(obj.MinPoint.Vector, obj.MaxPoint.Vector);
        }
    }
}
