using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Algorithms.Filling
{
    internal sealed class AreaFillingGlobalClusters<TModelInfo> : AreaFillingClustersBase<TModelInfo>
    {
        private readonly SimpleSpacialIndex<IClusterDefinition<TModelInfo>> map;

        public AreaFillingGlobalClusters(AreaDefinition fillarea, SimpleSpacialIndex<IClusterDefinition<TModelInfo>> map, IDensityDefinition densityDefinition, IReadOnlyList<IClusterDefinition<TModelInfo>> defaultClusters) 
            : base(fillarea, densityDefinition, defaultClusters)
        {
            this.map = map;
        }

        protected override IReadOnlyList<IClusterDefinition<TModelInfo>> Search(Vector2 start, Vector2 end)
        {
            return map.Search(start, end);
        }

    }
}
