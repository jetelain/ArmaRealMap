using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

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

        public override IClusterItemDefinition<TModelInfo>? SelectObjectToInsert(TerrainPoint point, IPointConditionContext conditionContext)
        {
            var cluster = SelectCluster(point, conditionContext);
            if (cluster != null)
            {
                return cluster.Models.GetRandom(area.Random, conditionContext);
            }
            return null;
        }

        private IClusterDefinition<TModelInfo>? SelectCluster(TerrainPoint point, IPointConditionContext conditionContext)
        {
            var potential = Search(point.Vector - ClusterSearchArea, point.Vector + ClusterSearchArea);
            if (potential.Count == 0)
            {
                return defaultClusters.GetRandom(area.Random, conditionContext);
            }
            return potential.GetEquiprobale(area.Random); // XXX: May apply condition again ? cluster positions has already applied condition
        }
    }
}
