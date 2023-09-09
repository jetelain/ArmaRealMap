using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets
{
    public class ObjectDefinition : IWithProbabilityAndCondition<IPointConditionContext>
    {
        public ObjectDefinition(Composition composition, double probability, PointCondition? condition = null)
        {
            Composition = composition;
            Probability = probability;
            Condition = condition;
        }

        public Composition Composition { get; }

        public double Probability { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PointCondition? Condition { get; }

        ICondition<IPointConditionContext>? IWithCondition<IPointConditionContext>.Condition => Condition;
    }
}
