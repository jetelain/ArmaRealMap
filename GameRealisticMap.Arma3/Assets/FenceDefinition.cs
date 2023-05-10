using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceDefinition : ISegmentsDefinition<Composition>
    {
        public FenceDefinition(double probability, List<StraightSegmentDefinition> straights)
        {
            Probability = probability;
            Straights = straights;
        }

        public double Probability { get; }

        public List<StraightSegmentDefinition> Straights { get; }

        IEnumerable<IStraightSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.Straights => Straights;
    }
}