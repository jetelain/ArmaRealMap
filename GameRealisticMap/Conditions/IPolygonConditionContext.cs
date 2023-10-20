using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    public interface IPolygonConditionContext : IConditionContext<TerrainPolygon>
    {
        float Area { get; }

        float MinElevation { get; }
        float MaxElevation { get; }
        float AvgElevation { get; }

        bool IsCommercial { get; }
        bool IsFarmyard { get; }
        bool IsIndustrial { get; }
        bool IsMilitary { get; }
        bool IsResidential { get; }
        bool IsRetail { get; }
    }
}