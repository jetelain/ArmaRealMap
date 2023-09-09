using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Conditions;
using Gemini.Framework;
using Gemini.Framework.Services;
using StringToExpression.Exceptions;
using StringToExpression.Util;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    [Export(typeof(IConditionTool))]
    internal class ConditionToolViewModel : Tool, IConditionTool
    {
        private ConditionVM? target;
        public ConditionToolViewModel()
        {
            DisplayName = "Tags editor";
            Criterias = typeof(IPointConditionContext).GetProperties()
                .Select(p => new CriteriaItem("Point", p.Name, p.PropertyType))
                .OrderBy(p => !p.IsBoolean)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public ConditionVM? Target
        { 
            get { return target; }
            set
            {
                target = value;
                if ( target != null)
                {
                    ConditionText = target.Condition;
                }
                else
                {
                    ConditionText = string.Empty;
                }
                NotifyOfPropertyChange(nameof(ConditionText));
                DoApply();
            }
        }

        public string ConditionText { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public List<ConditionToken> Tokens { get; private set; } = new List<ConditionToken>();

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public List<CriteriaItem> Criterias { get; }

        public Task Apply()
        {
            DoApply();
            return Task.CompletedTask;
        }

        private void DoApply()
        {
            PointCondition? condition = null;
            StringSegment? error = null;
            ErrorMessage = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(ConditionText))
                {
                    condition = new PointCondition(ConditionText);
                }
            }
            catch (ParseException ex)
            {
                ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                error = ex.ErrorSegment;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            try
            {
                Tokens = ConditionToken.Create(ConditionText, error);
            }
            catch
            {
                Tokens = new List<ConditionToken>();
            }
            if (!HasError && target != null)
            {
                target.SetParsedCondition(condition);
            }
            NotifyOfPropertyChange(nameof(ErrorMessage));
            NotifyOfPropertyChange(nameof(Tokens));
            NotifyOfPropertyChange(nameof(HasError));
        }

        public Task AddCriteria(CriteriaItem criteria)
        {
            var text = ConditionText;
            if (!string.IsNullOrEmpty(text) )
            {
                if(!text.EndsWith("!"))
                {
                    text += " && ";
                }
            }
            text += criteria.Name + criteria.InitText;
            ConditionText = text;
            NotifyOfPropertyChange(nameof(ConditionText));
            return Apply();
        }

    }
}
