namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterCollectionDefinition<out TModelInfo> : IWithProbability, IWithDensity
    {
        IReadOnlyList<IClusterDefinition<TModelInfo>> Clusters { get; }
    }
}
