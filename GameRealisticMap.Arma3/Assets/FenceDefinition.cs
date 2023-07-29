using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceDefinition : ISegmentsProportionDefinition<Composition>
    {
        public FenceDefinition(double probability, List<FenceSegmentDefinition> straights, bool isProportionForFullList = false, string label = "")
        {
            Probability = probability;
            Straights = straights;
            Label = label;
            IsProportionForFullList = isProportionForFullList;
        }

        public double Probability { get; }

        public List<FenceSegmentDefinition> Straights { get; }

        public string Label { get; }

        public bool IsProportionForFullList { get; }

        IEnumerable<IStraightSegmentProportionDefinition<Composition>> ISegmentsProportionDefinition<Composition>.Straights => Straights;
    }
}