using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with objects clusters defined at map scale
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaGlobalClusters<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly SimpleSpacialIndex<IClusterDefinition<TModelInfo>> map;
        private readonly IWithDensity densityDefinition;
        private readonly IReadOnlyList<IClusterDefinition<TModelInfo>> defaultClusters;

        public FillAreaGlobalClusters(IProgressSystem progress, SimpleSpacialIndex<IClusterDefinition<TModelInfo>> map, IWithDensity densityDefinition, IReadOnlyList<IClusterDefinition<TModelInfo>> defaultClusters)
            : base(progress)
        {
            this.map = map;
            this.densityDefinition = densityDefinition;
            this.defaultClusters = defaultClusters;
        }

        internal override AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea)
        {
            return new AreaFillingGlobalClusters<TModelInfo>(fillarea, map, densityDefinition, defaultClusters);
        }
    }
}
