using System.Collections.Generic;
using Caliburn.Micro;
using GameRealisticMap.Conditions;

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

        public PointCondition? ToCondition()
        {
            return condition;
        }

    }
}
