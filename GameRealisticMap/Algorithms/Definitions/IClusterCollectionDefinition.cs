using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterCollectionDefinition<out TModelInfo> : IDensityDefinition, IWithProbabilityAndCondition<IPolygonConditionContext>
    {
        IReadOnlyList<IClusterDefinition<TModelInfo>> Clusters { get; }
    }
}
