using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Test.Algorithms.Following
{
    internal class TestCornerOrEndSegmentDefinition : ICornerOrEndSegmentDefinition<string>
    {
        public TestCornerOrEndSegmentDefinition(string model, double probability = 1)
        {
            Model = model;
            Probability = probability;
        }

        public string Model { get; }

        public double Probability { get; }
    }
}