using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with objects clusters (clusters are specififc to each area)
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaLocalClusters<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly IReadOnlyCollection<IClusterCollectionDefinition<TModelInfo>> clustersDefinitions;

        public FillAreaLocalClusters(IProgressScope progress, IReadOnlyCollection<IClusterCollectionDefinition<TModelInfo>> clustersDefinitions)
            : base(progress)
        {
            this.clustersDefinitions = clustersDefinitions;
        }

        public override int FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons, IConditionEvaluator conditionEvaluator)
        {
            if (clustersDefinitions.Count > 0)
            {
                return base.FillPolygons(objects, polygons, conditionEvaluator);
            }
            return 0;
        }

        internal override AreaFillingBase<TModelInfo>? GenerateAreaSelectData(AreaDefinition fillarea, IConditionEvaluator conditionEvaluator)
        {
            var result = clustersDefinitions.GetRandom(fillarea.Random, conditionEvaluator.GetPolygonContext(fillarea.Polygon));
            if (result != null)
            {
                return new AreaFillingLocalClusters<TModelInfo>(fillarea, result, conditionEvaluator);
            }
            return null;
        }
    }
}
