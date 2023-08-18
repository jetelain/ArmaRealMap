using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets.Fences;

namespace GameRealisticMap.Arma3.Assets.Rows
{
    public class RowDefinition : IRowFillingDefinition<Composition>
    {
        public RowDefinition(double probability,
            double rowSpacing,
            List<FenceStraightSegmentDefinition>? segments = null,
            List<ItemDefinition>? objects = null,
            string label = "")
        {
            Probability = probability;
            Segments = segments ?? new List<FenceStraightSegmentDefinition>();
            Objects = objects ?? new List<ItemDefinition>();
            Label = label;
            RowSpacing = rowSpacing;
        }

        public double Probability { get; }

        public List<FenceStraightSegmentDefinition> Segments { get; }

        public List<ItemDefinition> Objects { get; }

        public string Label { get; }

        public double RowSpacing { get; }

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> IRowDefition<Composition>.Segments => Segments;

        IReadOnlyCollection<IItemDefinition<Composition>> IRowDefition<Composition>.Objects => Objects;
    }
}
