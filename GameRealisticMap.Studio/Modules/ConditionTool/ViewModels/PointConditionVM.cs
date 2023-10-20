using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public sealed class PointConditionVM : ConditionVMBase<PointCondition,IPointConditionContext,TerrainPoint>
    {

        public PointConditionVM(PointCondition? condition = null) : base(condition)
        {
        }

        internal override IPointConditionContext CreateContext(IConditionEvaluator evaluator, TerrainPoint geometry)
        {
            return evaluator.GetPointContext(geometry);
        }

        internal override IConditionSampleProvider<TerrainPoint> GetRandomProvider()
        {
            return new RandomSampleProvider();
        }

        internal override IConditionSampleProvider<TerrainPoint> GetViewportProvider(ITerrainEnvelope envelope)
        {
            return new ViewportSampleProvider(envelope);
        }

        internal override PointCondition Parse(string value)
        {
            return new PointCondition(value);
        }
    }
}
