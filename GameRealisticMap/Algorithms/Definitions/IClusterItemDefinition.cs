namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterItemDefinition<out TModelInfo> : IWithProbability
    {
        float Radius { get; }

        TModelInfo Model { get; }

        float? MaxZ { get; }

        float? MinZ { get; }

        float? MaxScale { get; }

        float? MinScale { get; }
    }
}
