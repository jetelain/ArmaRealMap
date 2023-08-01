using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceDefinition : ISegmentsDefinition<Composition>
    {
        public FenceDefinition(double probability, 
            List<FenceStraightSegmentDefinition> straights,
            List<FenceCornerOrEndDefinition>? leftCorners = null,
            List<FenceCornerOrEndDefinition>? rightCorners = null,
            List<FenceCornerOrEndDefinition>? ends = null,
            bool useAnySize = false, 
            string label = "")
        {
            Probability = probability;
            Straights = straights;
            Label = label;
            UseAnySize = useAnySize;
            LeftCorners = leftCorners ?? new List<FenceCornerOrEndDefinition>();
            RightCorners = rightCorners ?? new List<FenceCornerOrEndDefinition>();
            Ends = ends ?? new List<FenceCornerOrEndDefinition>();
        }

        public double Probability { get; }

        public List<FenceStraightSegmentDefinition> Straights { get; }

        public List<FenceCornerOrEndDefinition> LeftCorners { get; }

        public List<FenceCornerOrEndDefinition> RightCorners { get; }

        public List<FenceCornerOrEndDefinition> Ends { get; }

        public string Label { get; }

        public bool UseAnySize { get; }

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.LeftCorners => LeftCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.RightCorners => RightCorners;

        IReadOnlyCollection<ICornerOrEndSegmentDefinition<Composition>> ISegmentsDefinition<Composition>.Ends => Ends;

        IReadOnlyCollection<IStraightSegmentProportionDefinition<Composition>> ISegmentsDefinition<Composition>.Straights => Straights;
    }
}