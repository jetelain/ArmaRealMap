using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    internal class BasicCollectionDefinition : IBasicDefinition<Composition>
    {
        public BasicCollectionDefinition(IReadOnlyList<IClusterItemDefinition<Composition>> models, double probability, double minDensity, double maxDensity)
        {
            Models = models;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Models.CheckProbabilitySum();
        }

        public IReadOnlyList<IClusterItemDefinition<Composition>> Models { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }
    }
}
