using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public interface IRandomPointGenerator
    {
        TerrainPoint GetRandomPoint();
    }
}