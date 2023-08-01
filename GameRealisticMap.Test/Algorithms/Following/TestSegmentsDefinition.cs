using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Test.Algorithms.Following
{
    internal class TestSegmentsDefinition : ISegmentsProportionDefinition<string>
    {
        private readonly IReadOnlyCollection<TestStraightSegmentDefinition> straights;

        public TestSegmentsDefinition(params TestStraightSegmentDefinition[] straights)
        {
            this.straights = straights;
        }

        public TestSegmentsDefinition(IReadOnlyCollection<TestStraightSegmentDefinition> straights)
        {
            this.straights = straights;
        }

        public IEnumerable<IStraightSegmentProportionDefinition<string>> Straights => straights;

        public double Probability { get; set; }

        public bool UseAnySize { get; set; }
    }
}
