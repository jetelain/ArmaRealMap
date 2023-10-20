using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Algorithms.Following
{
    internal class TestSegmentsDefinition : ISegmentsDefinition<string>
    {
        private readonly IReadOnlyCollection<TestStraightSegmentDefinition> straights;
        private readonly IReadOnlyCollection<TestCornerOrEndSegmentDefinition> leftCorners;
        private readonly IReadOnlyCollection<TestCornerOrEndSegmentDefinition> rightCorners;
        private readonly IReadOnlyCollection<TestCornerOrEndSegmentDefinition> ends;

        public TestSegmentsDefinition(params TestStraightSegmentDefinition[] straights)
        {
            this.straights = straights;
            leftCorners = rightCorners = ends = new List<TestCornerOrEndSegmentDefinition>();
        }

        public TestSegmentsDefinition(TestCornerOrEndSegmentDefinition? left, TestCornerOrEndSegmentDefinition? right, TestCornerOrEndSegmentDefinition? end, params TestStraightSegmentDefinition[] straights)
        {
            this.straights = straights;
            leftCorners = left != null ? new List<TestCornerOrEndSegmentDefinition>() { left } : new List<TestCornerOrEndSegmentDefinition>();
            rightCorners = right != null ? new List<TestCornerOrEndSegmentDefinition>() { right } : new List<TestCornerOrEndSegmentDefinition>();
            ends = end != null ? new List<TestCornerOrEndSegmentDefinition>() { end } : new List<TestCornerOrEndSegmentDefinition>();
        }

        public IReadOnlyCollection<IStraightSegmentProportionDefinition<string>> Straights => straights;

        public double Probability { get; set; }

        public bool UseAnySize { get; set; }

        public IReadOnlyCollection<ICornerOrEndSegmentDefinition<string>> LeftCorners => leftCorners;

        public IReadOnlyCollection<ICornerOrEndSegmentDefinition<string>> RightCorners => rightCorners;

        public IReadOnlyCollection<ICornerOrEndSegmentDefinition<string>> Ends => ends;

        public ICondition<IPathConditionContext>? Condition => null;
    }
}
