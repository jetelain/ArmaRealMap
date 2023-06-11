using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterCollectionDefinition : IClusterCollectionDefinition<Composition>
    {
        [JsonConstructor]
        public ClusterCollectionDefinition(IReadOnlyList<ClusterDefinition> clusters, double probability, double minDensity, double maxDensity, string label = "")
        {
            Label = label;
            Clusters = clusters;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Clusters.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterDefinition> Clusters { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }

        public string Label { get; }

        [JsonIgnore]
        IReadOnlyList<IClusterDefinition<Composition>> IClusterCollectionDefinition<Composition>.Clusters => Clusters;
    }
}
