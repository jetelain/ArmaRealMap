namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterItemDefinition<out TModelInfo> : IItemDefinition<TModelInfo>
    {
        float FitRadius { get; }
    }
}
