namespace GameRealisticMap.Conditions
{
    public interface IPointConditionContext
    {
        float DistanceToOcean { get; }
        float DistanceToRoad { get; }
        float Elevation { get; }
        bool IsCommercial { get; }
        bool IsFarmyard { get; }
        bool IsIndustrial { get; }
        bool IsMilitary { get; }
        bool IsOcean { get; }
        bool IsResidential { get; }
        bool IsRetail { get; }
        bool IsRoadMotorway { get; }
        bool IsRoadPath { get; }
        bool IsRoadPrimary { get; }
        bool IsRoadSecondary { get; }
        bool IsRoadSimple { get; }
        bool IsUrban { get; }
        float Slope { get; }
    }
}