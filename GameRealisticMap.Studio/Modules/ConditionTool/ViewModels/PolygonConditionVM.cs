using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

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
            throw new System.NotImplementedException(); // FIXME
        }

        internal override IConditionSampleProvider<TerrainPolygon> GetViewportProvider(ITerrainEnvelope envelope)
        {
            throw new System.NotImplementedException(); // FIXME
        }

        internal override PolygonCondition Parse(string value)
        {
            return new PolygonCondition(value);
        }
    }
}
