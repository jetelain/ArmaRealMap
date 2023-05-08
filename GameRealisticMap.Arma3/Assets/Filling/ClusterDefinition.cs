using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterDefinition : IClusterDefinition<Composition>
    {
        public ClusterDefinition(ClusterItemDefinition model, double probability)
        {
            Models = new List<ClusterItemDefinition>(1) { model };
            Probability = probability;
            Models.CheckProbabilitySum();
        }

        [JsonConstructor]
        public ClusterDefinition(IReadOnlyList<ClusterItemDefinition> models, double probability)
        {
            Models = models;
            Probability = probability;
            Models.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterItemDefinition> Models { get; }

        public double Probability { get; }

        IReadOnlyList<IClusterItemDefinition<Composition>> IClusterDefinition<Composition>.Models => Models;
    }
}
