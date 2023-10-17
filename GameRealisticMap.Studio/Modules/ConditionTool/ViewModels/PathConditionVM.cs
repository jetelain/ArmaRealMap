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
            throw new System.NotImplementedException(); // FIXME
        }

        internal override IConditionSampleProvider<TerrainPath> GetViewportProvider(ITerrainEnvelope envelope)
        {
            throw new System.NotImplementedException(); // FIXME
        }

        internal override PathCondition Parse(string value)
        {
            return new PathCondition(value);
        }
    }
}
