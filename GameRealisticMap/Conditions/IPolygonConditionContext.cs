namespace GameRealisticMap.Conditions
{
    public interface IPolygonConditionContext
    {
        float Area { get; }

        float MinElevation { get; }
        float MaxElevation { get; }
        float AvgElevation { get; }
    }
}