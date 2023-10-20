using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public sealed class BasicCollectionDefinition : IBasicDefinition<Composition>
    {
        [JsonConstructor]
        public BasicCollectionDefinition(IReadOnlyList<ClusterItemDefinition> models, double probability, double minDensity, double maxDensity, string label = "", PolygonCondition? condition = null)
        {
            Models = models;
            Probability = probability;
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            Label = label;
            Condition = condition;
            Models.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterItemDefinition> Models { get; }

        public double Probability { get; }

        public double MinDensity { get; }

        public double MaxDensity { get; }

        public string Label { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PolygonCondition? Condition { get; }

        [JsonIgnore]
        IReadOnlyList<IClusterItemDefinition<Composition>> IBasicDefinition<Composition>.Models => Models;

        [JsonIgnore]
        ICondition<IPolygonConditionContext>? IWithCondition<IPolygonConditionContext>.Condition => Condition;
    }
}
