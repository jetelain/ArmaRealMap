using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    internal class ClusterDefinition : IClusterDefinition<Composition>
    {
        public ClusterDefinition(IReadOnlyList<IClusterItemDefinition<Composition>> models, double probability)
        {
            Models = models;
            Probability = probability;
        }

        public IReadOnlyList<IClusterItemDefinition<Composition>> Models { get; }

        public double Probability { get; }
    }
}
