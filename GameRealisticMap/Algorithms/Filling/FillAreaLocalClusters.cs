using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with objects clusters (clusters are specififc to each area)
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaLocalClusters<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly IReadOnlyCollection<IClusterCollectionDefinition<TModelInfo>> clustersDefinitions;

        public FillAreaLocalClusters(IProgressSystem progress, IReadOnlyCollection<IClusterCollectionDefinition<TModelInfo>> clustersDefinitions)
            : base(progress)
        {
            this.clustersDefinitions = clustersDefinitions;
        }

        public override int FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons)
        {
            if (clustersDefinitions.Count > 0)
            {
                return base.FillPolygons(objects, polygons);
            }
            return 0;
        }

        internal override AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea)
        {
            return new AreaFillingLocalClusters<TModelInfo>(fillarea, clustersDefinitions.GetRandom(fillarea.Random));
        }
    }
}
