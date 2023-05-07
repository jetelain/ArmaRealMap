using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public interface IModelPosition
    {
        float Angle { get; }

        TerrainPoint Center { get; }

        float RelativeElevation { get; }

        float Scale { get; }
    }
}