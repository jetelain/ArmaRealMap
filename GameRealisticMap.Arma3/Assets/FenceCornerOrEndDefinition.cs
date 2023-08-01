using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class FenceCornerOrEndDefinition : ICornerOrEndSegmentDefinition<Composition>
    {
        public FenceCornerOrEndDefinition(Composition model, double probability)
        {
            Model = model;
            Probability = probability;
        }

        public Composition Model { get; }

        public double Probability { get; }

    }
}