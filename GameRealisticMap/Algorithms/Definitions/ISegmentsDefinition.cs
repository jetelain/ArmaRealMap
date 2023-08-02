namespace GameRealisticMap.Algorithms.Definitions
{
    public interface ISegmentsDefinition<out TModelInfo> : IWithProbability
    {
        IReadOnlyCollection<IStraightSegmentProportionDefinition<TModelInfo>> Straights { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> LeftCorners { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> RightCorners { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> Ends { get; }

        bool UseAnySize { get; }
    }
}
