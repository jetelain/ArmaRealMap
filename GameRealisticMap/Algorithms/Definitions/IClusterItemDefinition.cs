using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterItemDefinition<out TModelInfo> : IItemDefinition<TModelInfo>, IWithProbabilityAndCondition<IPointConditionContext>
    {
        float FitRadius { get; }
    }
}
