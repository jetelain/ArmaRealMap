using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public sealed class PathConditionVM 
        : ConditionVMBase<PathCondition, IPathConditionContext, TerrainPath>
    {
        public PathConditionVM(PathCondition? condition = null) : base(condition)
        {
        }

        internal override IPathConditionContext CreateContext(IConditionEvaluator evaluator, TerrainPath geometry)
        {
            return evaluator.GetPathContext(geometry);
        }

        internal override IConditionSampleProvider<TerrainPath> GetRandomProvider()
        {
            return new RandomSampleProvider();
        }

        internal override IConditionSampleProvider<TerrainPath> GetViewportProvider(ITerrainEnvelope envelope)
        {
            return new ViewportSampleProvider(envelope);
        }

        internal override PathCondition Parse(string value)
        {
            return new PathCondition(value);
        }
    }
}
