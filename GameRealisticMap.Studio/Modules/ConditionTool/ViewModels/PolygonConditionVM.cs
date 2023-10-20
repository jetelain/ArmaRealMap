using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public sealed class PolygonConditionVM 
        : ConditionVMBase<PolygonCondition, IPolygonConditionContext, TerrainPolygon>
    {

        public PolygonConditionVM(PolygonCondition? condition = null) : base(condition)
        {
        }

        internal override IPolygonConditionContext CreateContext(IConditionEvaluator evaluator, TerrainPolygon geometry)
        {
            return evaluator.GetPolygonContext(geometry);
        }

        internal override IConditionSampleProvider<TerrainPolygon> GetRandomProvider()
        {
            return new RandomSampleProvider();
        }

        internal override IConditionSampleProvider<TerrainPolygon> GetViewportProvider(ITerrainEnvelope envelope)
        {
            return new ViewportSampleProvider(envelope);
        }

        internal override PolygonCondition Parse(string value)
        {
            return new PolygonCondition(value);
        }
    }
}
