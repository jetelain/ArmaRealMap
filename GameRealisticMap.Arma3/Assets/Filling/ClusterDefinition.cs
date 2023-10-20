using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterDefinition : IClusterDefinition<Composition>
    {
        public ClusterDefinition(ClusterItemDefinition model, double probability, PointCondition? condition = null)
        {
            Models = new List<ClusterItemDefinition>(1) { model };
            Probability = probability;
            Condition = condition;
            Models.CheckProbabilitySum();
        }

        [JsonConstructor]
        public ClusterDefinition(IReadOnlyList<ClusterItemDefinition> models, double probability, PointCondition? condition = null)
        {
            Models = models;
            Probability = probability;
            Condition = condition;
            Models.CheckProbabilitySum();
        }

        public IReadOnlyList<ClusterItemDefinition> Models { get; }

        public double Probability { get; }

        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
        public PointCondition? Condition { get; }

        IReadOnlyList<IClusterItemDefinition<Composition>> IClusterDefinition<Composition>.Models => Models;

        [JsonIgnore]
        ICondition<IPointConditionContext>? IWithCondition<IPointConditionContext>.Condition => Condition;
    }
}
