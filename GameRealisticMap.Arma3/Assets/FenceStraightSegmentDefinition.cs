using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    /// <summary>
    /// Straight segment, aligned east-west, centered on (0,0)
    /// </summary>
    public class FenceStraightSegmentDefinition : StraightSegmentDefinition, IStraightSegmentProportionDefinition<Composition>
    {
        public FenceStraightSegmentDefinition(Composition model, float size, float proportion = 1)
            : base(model, size) 
        {
            Proportion = proportion > 0 ? proportion : 1;
        }

        public float Proportion { get; }
    }
}
