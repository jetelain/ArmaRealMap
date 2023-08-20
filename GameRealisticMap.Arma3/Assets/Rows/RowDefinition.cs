using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets.Fences;

namespace GameRealisticMap.Arma3.Assets.Rows
{
    public class RowDefinition : IRowFillingDefinition<Composition>
    {
        public RowDefinition(double probability,
            double rowSpacing,
            List<FenceStraightSegmentDefinition>? straights = null,
            List<ItemDefinition>? objects = null,
            string label = "")
        {
            Probability = probability;
            Straights = straights ?? new List<FenceStraightSegmentDefinition>();
            Objects = objects ?? new List<ItemDefinition>();
            Label = label;
            RowSpacing = rowSpacing;
        }

        public double Probability { get; }

        public List<FenceStraightSegmentDefinition> Straights { get; }

        public List<ItemDefinition> Objects { get; }

        public string Label { get; }

        public double RowSpacing { get; }

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> IRowDefition<Composition>.Straights => Straights;

        IReadOnlyCollection<IItemDefinition<Composition>> IRowDefition<Composition>.Objects => Objects;
    }
}
