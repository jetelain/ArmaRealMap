using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterCollectionDefinition : IClusterCollectionDefinition<Composition>
    {
        [JsonConstructor]
        public ClusterCollectionDefinition(IReadOnlyList<ClusterDefinition> clusters, double probability, double minDensity, double maxDensity, string label = "", PolygonCondition? condition = null)
        {
            Label = label;
            Clusters = clusters;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Condition = condition;
            Clusters.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterDefinition> Clusters { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }

        public string Label { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PolygonCondition? Condition { get; }

        [JsonIgnore]
        IReadOnlyList<IClusterDefinition<Composition>> IClusterCollectionDefinition<Composition>.Clusters => Clusters;

        [JsonIgnore]
        ICondition<IPolygonConditionContext>? IWithCondition<IPolygonConditionContext>.Condition => Condition;
    }
}
