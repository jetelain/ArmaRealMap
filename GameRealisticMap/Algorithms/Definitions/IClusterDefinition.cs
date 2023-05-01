namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterDefinition<out TModelInfo> : IWithProbability
    {
        IReadOnlyList<IModelDefinition<TModelInfo>> Models { get; }
    }
}
