using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    public interface IPolygonConditionContext : IConditionContext<TerrainPolygon>
    {
        float Area { get; }

        float MinElevation { get; }
        float MaxElevation { get; }
        float AvgElevation { get; }
    }
}