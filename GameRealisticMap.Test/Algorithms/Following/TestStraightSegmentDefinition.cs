using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Test.Algorithms.Following
{
    internal class TestStraightSegmentDefinition : IStraightSegmentProportionDefinition<string>
    {
        public TestStraightSegmentDefinition(string model, float size, float proportion = 1)
        {
            Model = model;
            Size = size;
            Proportion = proportion;
        }

        public string Model { get; }

        public float Size { get; }

        public float Proportion { get; }
    }
}
