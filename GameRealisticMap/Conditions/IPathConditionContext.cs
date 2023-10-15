namespace GameRealisticMap.Conditions
{
    public interface IPathConditionContext
    {
        float Length { get; }

        float MinElevation { get; }
        float MaxElevation { get; }
        float AvgElevation { get; }

    }
}