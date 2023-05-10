namespace GameRealisticMap.Algorithms.Definitions
{
    public interface ISegmentsDefinition<out TModelInfo> : IWithProbability
    {
        IEnumerable<IStraightSegmentDefinition<TModelInfo>> Straights { get; }
    }
}
