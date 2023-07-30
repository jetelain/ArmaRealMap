using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceDefinition : ISegmentsProportionDefinition<Composition>
    {
        public FenceDefinition(double probability, List<FenceSegmentDefinition> straights, bool useAnySize = false, string label = "")
        {
            Probability = probability;
            Straights = straights;
            Label = label;
            UseAnySize = useAnySize;
        }

        public double Probability { get; }

        public List<FenceSegmentDefinition> Straights { get; }

        public string Label { get; }

        public bool UseAnySize { get; }

        IEnumerable<IStraightSegmentProportionDefinition<Composition>> ISegmentsProportionDefinition<Composition>.Straights => Straights;
    }
}