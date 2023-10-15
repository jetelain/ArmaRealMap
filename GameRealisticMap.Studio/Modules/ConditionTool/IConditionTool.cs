using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.ConditionTool
{
    internal interface IConditionTool : ITool
    {
        void SetTarget<TCondition, TContext, TGeometry>(ConditionVMBase<TCondition, TContext, TGeometry> target) 
            where TCondition : class, ICondition<TContext>
            where TContext : IConditionContext<TGeometry>
            where TGeometry : ITerrainEnvelope;
    }
}
