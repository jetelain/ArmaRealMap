using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    /// <summary>
    /// Abstract base view-model for condition expression editors.
    /// Manages the current <typeparamref name="TCondition"/> object, parses/regenerates
    /// syntax tokens for the editor highlight display, and delegates test execution to an
    /// <see cref="IConditionSampleProvider{TGeometry}"/>.
    /// Concrete subclasses: <see cref="PointConditionVM"/>, <see cref="PathConditionVM"/>,
    /// <see cref="PolygonConditionVM"/>.
    /// </summary>
    /// <typeparam name="TCondition">The strongly-typed condition class (e.g. <c>PointCondition</c>).</typeparam>
    /// <typeparam name="TContext">The evaluation context type for the condition.</typeparam>
    /// <typeparam name="TGeometry">The geometry type the condition filters (point / path / polygon).</typeparam>
    public abstract class ConditionVMBase<TCondition,TContext,TGeometry> : PropertyChangedBase, IConditionVM 
        where TCondition : class, ICondition<TContext>
        where TContext : IConditionContext<TGeometry>
        where TGeometry : ITerrainEnvelope
    {

        private TCondition? condition;

        public ConditionVMBase(TCondition? condition = null) 
        {
            this.condition = condition;
            UpdateTokens();
        }

        private void UpdateTokens()
        {
            Tokens = ConditionToken.Create(condition?.OriginalString);
            NotifyOfPropertyChange(nameof(Tokens));
        }

        internal IConditionSampleProvider<TGeometry>? SamplePointProvider { get; set; }

        internal abstract TCondition Parse(string value);

        public List<ConditionToken> Tokens { get; private set; } = new List<ConditionToken>();

        public string Condition
        {
            get { return condition?.OriginalString ?? string.Empty; }
            set 
            { 
                if (Condition != value)
                {
                    condition = string.IsNullOrEmpty(value) ? null : Parse(value);
                    NotifyOfPropertyChange();
                    UpdateTokens();
                } 
            }
        }

        public void SetParsedCondition(TCondition? condition)
        {
            this.condition = condition;
            UpdateTokens();
        }

        public TCondition? ToDefinition()
        {
            return condition;
        }

        public Task Edit()
        {
            var tool = IoC.Get<IConditionTool>();
            IoC.Get<IShell>().ShowTool(tool);
            tool.SetTarget(this);
            return Task.CompletedTask;
        }

        internal abstract TContext CreateContext(IConditionEvaluator evaluator, TGeometry geometry);
        internal virtual IConditionSampleProvider<TGeometry> GetDefaultProvider()
        {
            return SamplePointProvider ?? GetRandomProvider();
        }
        internal abstract IConditionSampleProvider<TGeometry> GetRandomProvider();
        internal abstract IConditionSampleProvider<TGeometry> GetViewportProvider(ITerrainEnvelope envelope);
    }
}
