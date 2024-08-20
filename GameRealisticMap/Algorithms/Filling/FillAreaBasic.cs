using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with randomly choosen models
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaBasic<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly IReadOnlyCollection<IBasicDefinition<TModelInfo>> basicDefinitions;

        public FillAreaBasic(IProgressScope progress, IReadOnlyCollection<IBasicDefinition<TModelInfo>> basicDefinitions)
            : base(progress)
        {
            this.basicDefinitions = basicDefinitions;
        }

        public override int FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons, IConditionEvaluator conditionEvaluator)
        {
            if (basicDefinitions.Count > 0)
            {
                return base.FillPolygons(objects, polygons, conditionEvaluator);
            }
            return 0;
        }

        internal override AreaFillingBase<TModelInfo>? GenerateAreaSelectData(AreaDefinition fillarea, IConditionEvaluator conditionEvaluator)
        {
            var result = basicDefinitions.GetRandom(fillarea.Random, conditionEvaluator.GetPolygonContext(fillarea.Polygon));
            if (result != null)
            {
                return new AreaFillingBasic<TModelInfo>(fillarea, result);
            }
            return null;
        }
    }
}
