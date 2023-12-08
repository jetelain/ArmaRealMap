using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IBasicDefinition<out TModelInfo> : IWithProbabilityAndCondition<IPolygonConditionContext>, IDensityDefinition
    {
        IReadOnlyList<IClusterItemDefinition<TModelInfo>> Models { get; }
    }
}
