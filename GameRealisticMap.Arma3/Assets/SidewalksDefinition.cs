﻿using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets
{
    public class SidewalksDefinition : ISegmentsDefinition<Composition>
    {
        public SidewalksDefinition(double probability,
            List<FenceStraightSegmentDefinition>? straights,
            List<FenceCornerOrEndDefinition>? leftCorners = null,
            List<FenceCornerOrEndDefinition>? rightCorners = null,
            List<FenceCornerOrEndDefinition>? ends = null,
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
            Condition = condition;
        }

        public double Probability { get; }

        public List<FenceStraightSegmentDefinition> Straights { get; }

        public List<FenceCornerOrEndDefinition> LeftCorners { get; }

        public List<FenceCornerOrEndDefinition> RightCorners { get; }

        public List<FenceCornerOrEndDefinition> Ends { get; }

        public string Label { get; }

        public bool UseAnySize { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PathCondition? Condition { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.LeftCorners => LeftCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.RightCorners => RightCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.Ends => Ends;

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> ISegmentsDefinition<Composition>.Straights => Straights;

        [JsonIgnore]
        ICondition<IPathConditionContext>? IWithCondition<IPathConditionContext>.Condition => Condition;
    }
}