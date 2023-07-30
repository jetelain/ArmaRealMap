namespace GameRealisticMap.Algorithms.Definitions
{
    public interface ISegmentsProportionDefinition<out TModelInfo> : IWithProbability
    {
        IEnumerable<IStraightSegmentProportionDefinition<TModelInfo>> Straights { get; }

        bool UseAnySize { get; }
    }
}
