namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IModelDefinition<out TModelInfo> : IWithProbability
    {
        float Radius { get; }

        TModelInfo Model { get; }

        float? MaxZ { get; }

        float? MinZ { get; }
    }
}
