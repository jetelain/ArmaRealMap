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
        public ConditionToolViewModel()
        {
            DisplayName = "Condition";
            Criterias = typeof(PointConditionContext).GetProperties()
                .Select(p => new CriteriaItem("Point", p.Name, p.PropertyType))
                .OrderBy(p => !p.IsBoolean)
                .ThenBy(p => p.Name)
                .ToList();
        }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        //public ConditionVM Model1 { get; set; } = new ConditionVM(new PointCondition("Elevation > 1000 && IsUrban && !IsResidential"));

        public string ConditionText { get; set; } = "IsUrban && !IsResidential";

        public string ErrorMessage { get; set; } = string.Empty;

        public List<ConditionToken> Tokens { get; private set; } = new List<ConditionToken>();

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public List<CriteriaItem> Criterias { get; }

        public Task Apply()
        {
            StringSegment? error = null;
            ErrorMessage = string.Empty;
            try
            {
                if ( !string.IsNullOrEmpty(ConditionText) )
                {
                    var condition = new PointCondition(ConditionText);
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

            NotifyOfPropertyChange(nameof(ErrorMessage));
            NotifyOfPropertyChange(nameof(Tokens));
            NotifyOfPropertyChange(nameof(HasError));
            return Task.CompletedTask;
        }

        public Task AddCriteria(CriteriaItem criteria)
        {
            var text = ConditionText;
            if (!string.IsNullOrEmpty(text) )
            {
                text += " && ";
            }
            text += criteria.Name + criteria.InitText;
            ConditionText = text;
            NotifyOfPropertyChange(nameof(ConditionText));
            return Apply();
        }

    }
}
