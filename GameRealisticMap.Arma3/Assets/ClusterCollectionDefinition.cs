using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    internal class ClusterCollectionDefinition : IClusterCollectionDefinition<Composition>
    {
        public ClusterCollectionDefinition(IReadOnlyList<IClusterDefinition<Composition>> clusters, double probability, double minDensity, double maxDensity)
        {
            Clusters = clusters;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Clusters.CheckProbabilitySum();
        }

        public IReadOnlyList<IClusterDefinition<Composition>> Clusters { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }
    }
}
