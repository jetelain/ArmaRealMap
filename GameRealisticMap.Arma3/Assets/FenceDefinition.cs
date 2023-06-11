using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceDefinition : ISegmentsDefinition<Composition>
    {
        public FenceDefinition(double probability, List<StraightSegmentDefinition> straights, string label = "")
        {
            Probability = probability;
            Straights = straights;
            Label = label;
        }

        public double Probability { get; }

        public List<StraightSegmentDefinition> Straights { get; }

        public string Label { get; }

        IEnumerable<IStraightSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.Straights => Straights;
    }
}