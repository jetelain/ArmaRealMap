namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterDefinition<out TModelInfo> : IWithProbability
    {
        IReadOnlyList<IClusterItemDefinition<TModelInfo>> Models { get; }
    }
}
