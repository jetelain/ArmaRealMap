using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IClusterCollectionDefinition<out TModelInfo> : IWithDensity, IWithProbabilityAndCondition<IPolygonConditionContext>
    {
        IReadOnlyList<IClusterDefinition<TModelInfo>> Clusters { get; }
    }
}
