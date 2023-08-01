namespace GameRealisticMap.Algorithms.Definitions
{
    public interface ICornerOrEndSegmentDefinition<out TModelInfo> : IWithProbability
    {
        TModelInfo Model { get; }

        // XXX: Size can become useful
    }
}
