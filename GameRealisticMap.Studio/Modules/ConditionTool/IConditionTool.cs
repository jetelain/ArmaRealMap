using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.ConditionTool
{
    /// <summary>
    /// Dockable condition-editor tool pane. Accepts a <see cref="ConditionVMBase{TCondition,TContext,TGeometry}"/>
    /// target so the editor knows which condition expression it is currently editing and
    /// can display syntax tokens and test results in context.
    /// </summary>
    internal interface IConditionTool : ITool
    {
        void SetTarget<TCondition, TContext, TGeometry>(ConditionVMBase<TCondition, TContext, TGeometry> target) 
            where TCondition : class, ICondition<TContext>
            where TContext : IConditionContext<TGeometry>
            where TGeometry : ITerrainEnvelope;
    }
}
