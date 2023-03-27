using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap
{
    internal class FillCluster
    {
        public FillCluster(TerrainPoint point, SingleObjetInfos obj)
        {
            Point = point;
            Object = obj;
        }

        public TerrainPoint Point { get; }

        public SingleObjetInfos Object { get; }
    }
}