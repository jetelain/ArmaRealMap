using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Assets
{
    public class ObjectDefinition : IWithProbabilityAndCondition<PointConditionContext>
    {
        public ObjectDefinition(Composition composition, double probability, PointCondition? condition = null)
        {
            Composition = composition;
            Probability = probability;
            Condition = condition;
        }

        public Composition Composition { get; }

        public double Probability { get; }

        public PointCondition? Condition { get; }

        ICondition<PointConditionContext>? IWithCondition<PointConditionContext>.Condition => Condition;
    }
}
