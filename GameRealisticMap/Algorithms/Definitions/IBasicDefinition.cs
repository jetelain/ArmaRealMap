namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IBasicDefinition<out TModelInfo> : IWithProbability, IWithDensity
    {
        IReadOnlyList<IModelDefinition<TModelInfo>> Models { get; }
    }
}
