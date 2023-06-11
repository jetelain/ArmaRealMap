using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Algorithms.Filling
{
    internal sealed class AreaFillingLocalClusters<TModelInfo> : AreaFillingClustersBase<TModelInfo>
    {
        private readonly SimpleSpacialIndex<IClusterDefinition<TModelInfo>> map;

        public AreaFillingLocalClusters(AreaDefinition fillarea, IClusterCollectionDefinition<TModelInfo> definition) 
            : base(fillarea, definition, definition.Clusters)
        {
            var size = fillarea.MaxPoint.Vector - fillarea.MinPoint.Vector;
            var clusterCount = Math.Max(Density * size.X * size.Y, 100);
            map = new (fillarea.MinPoint.Vector, fillarea.MaxPoint.Vector);
            foreach (var cluster in definition.Clusters)
            {
                var count = clusterCount * cluster.Probability;
                for (int i = 0; i < count; ++i)
                {
                    map.Insert(fillarea.GetRandomPoint().Vector, cluster);
                }
            }
        }

        protected override IReadOnlyList<IClusterDefinition<TModelInfo>> Search(Vector2 start, Vector2 end)
        {
            return map.Search(start, end);
        }

    }
}
