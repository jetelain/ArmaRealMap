using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    public interface IPathConditionContext : IConditionContext<TerrainPath>
    {
        float Length { get; }

        float MinElevation { get; }
        float MaxElevation { get; }
        float AvgElevation { get; }

    }
}