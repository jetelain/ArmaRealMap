using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class BasicCollectionDefinition : IBasicDefinition<Composition>
    {
        [JsonConstructor]
        public BasicCollectionDefinition(IReadOnlyList<ClusterItemDefinition> models, double probability, double minDensity, double maxDensity)
        {
            Models = models;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Models.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterItemDefinition> Models { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }

        [JsonIgnore]
        IReadOnlyList<IClusterItemDefinition<Composition>> IBasicDefinition<Composition>.Models => Models;
    }
}
