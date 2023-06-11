using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    public class ObjectDefinition : IWithProbability
    {
        public ObjectDefinition(Composition composition, double probability)
        {
            Composition = composition;
            Probability = probability;
        }

        public Composition Composition { get; }

        public double Probability { get; }
    }
}
