using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterDefinition<out TModelInfo> : IWithProbabilityAndCondition<IPointConditionContext>
    {
        IReadOnlyList<IClusterItemDefinition<TModelInfo>> Models { get; }
    }
}
