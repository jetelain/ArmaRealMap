namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IBasicDefinition<out TModelInfo> : IWithProbability, IWithDensity
    {
        IReadOnlyList<IClusterItemDefinition<TModelInfo>> Models { get; }
    }
}
