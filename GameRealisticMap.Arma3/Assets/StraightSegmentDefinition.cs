using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    /// <summary>
    /// Straight segment, aligned to north, centered on (0,0)
    /// </summary>
    public class StraightSegmentDefinition : IStraightSegmentDefinition<Composition>
    {
        public StraightSegmentDefinition(Composition model, float size)
        {
            Model = model;
            Size = size;
        }

        public Composition Model { get; }

        public float Size { get; }
    }
}
