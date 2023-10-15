using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets.Fences
{
    public class FenceDefinition : ISegmentsDefinition<Composition>, IRowDefition<Composition>, IWithProbabilityAndCondition<IPathConditionContext>
    {
        public FenceDefinition(double probability,
            List<FenceStraightSegmentDefinition>? straights,
            List<FenceCornerOrEndDefinition>? leftCorners = null,
            List<FenceCornerOrEndDefinition>? rightCorners = null,
            List<FenceCornerOrEndDefinition>? ends = null,
            List<ItemDefinition>? objects = null,
            bool useAnySize = false,
            string label = "",
            PathCondition? condition = null)
        {
            Probability = probability;
            Straights = straights ?? new List<FenceStraightSegmentDefinition>();
            Label = label;
            UseAnySize = useAnySize;
            LeftCorners = leftCorners ?? new List<FenceCornerOrEndDefinition>();
            RightCorners = rightCorners ?? new List<FenceCornerOrEndDefinition>();
            Ends = ends ?? new List<FenceCornerOrEndDefinition>();
            Objects = objects ?? new List<ItemDefinition>();
            Condition = condition;
        }

        public double Probability { get; }

        public List<FenceStraightSegmentDefinition> Straights { get; }

        public List<FenceCornerOrEndDefinition> LeftCorners { get; }

        public List<FenceCornerOrEndDefinition> RightCorners { get; }

        public List<FenceCornerOrEndDefinition> Ends { get; }

        public List<ItemDefinition> Objects { get; }

        public string Label { get; }

        public bool UseAnySize { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PathCondition? Condition { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.LeftCorners => LeftCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.RightCorners => RightCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.Ends => Ends;

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> ISegmentsDefinition<Composition>.Straights => Straights;

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> IRowDefition<Composition>.Straights => Straights;

        IReadOnlyCollection<IItemDefinition<Composition>> IRowDefition<Composition>.Objects => Objects;

        ICondition<IPathConditionContext>? IWithCondition<IPathConditionContext>.Condition => Condition;
    }
}