using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface ISegmentsDefinition<out TModelInfo> : IWithProbabilityAndCondition<IPathConditionContext>
    {
        IReadOnlyCollection<IStraightSegmentProportionDefinition<TModelInfo>> Straights { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> LeftCorners { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> RightCorners { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<TModelInfo>> Ends { get; }

        bool UseAnySize { get; }
    }
}
