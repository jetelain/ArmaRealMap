using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Conditions;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public class ConditionVM : PropertyChangedBase
    {

        private PointCondition? condition;

        public ConditionVM(PointCondition? condition = null) 
        {
            this.condition = condition;
            UpdateTokens();
        }

        private void UpdateTokens()
        {
            Tokens = ConditionToken.Create(condition?.OriginalString);
            NotifyOfPropertyChange(nameof(Tokens));
        }

        internal ISamplePointProvider? SamplePointProvider { get; set; }

        public List<ConditionToken> Tokens { get; private set; } = new List<ConditionToken>();

        public string Condition
        {
            get { return condition?.OriginalString ?? string.Empty; }
            set 
            { 
                if (Condition != value)
                {
                    condition = string.IsNullOrEmpty(value) ? null : new PointCondition(value);
                    NotifyOfPropertyChange();
                    UpdateTokens();
                } 
            }
        }

        public void SetParsedCondition(PointCondition? condition)
        {
            this.condition = condition;
            UpdateTokens();
        }

        public PointCondition? ToDefinition()
        {
            return condition;
        }

        public Task Edit()
        {
            var tool = IoC.Get<IConditionTool>();
            IoC.Get<IShell>().ShowTool(tool);
            tool.Target = this;
            return Task.CompletedTask;
        }
    }
}
