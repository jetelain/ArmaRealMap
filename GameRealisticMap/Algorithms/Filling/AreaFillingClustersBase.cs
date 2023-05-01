using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Algorithms.Filling
{
    internal abstract class AreaFillingClustersBase<TModelInfo> : AreaFillingBase<TModelInfo>
    {
        private static readonly Vector2 ClusterSearchArea = new Vector2(50, 50);

        private readonly IReadOnlyList<IClusterDefinition<TModelInfo>> defaultClusters;

        public AreaFillingClustersBase(AreaDefinition fillarea, IWithDensity densityDefinition, IReadOnlyList<IClusterDefinition<TModelInfo>> defaultClusters) 
            : base(fillarea, densityDefinition)
        {
            this.defaultClusters = defaultClusters;
        }

        protected abstract IReadOnlyList<IClusterDefinition<TModelInfo>> Search(Vector2 start, Vector2 end);

        public override IModelDefinition<TModelInfo> SelectObjectToInsert(TerrainPoint point)
        {
            var potential = Search(point.Vector - ClusterSearchArea, point.Vector + ClusterSearchArea);
            if (potential.Count == 0)
            {
                potential = defaultClusters;
            }
            var cluster = potential.GetRandom(area.Random);
            return cluster.Models.GetRandom(area.Random);
        }
    }
}
