using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using StringToExpression.Exceptions;
using StringToExpression.Util;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal sealed class ConditionToolWorker<TCondition, TContext, TGeometry> : IConditionToolWorker
        where TCondition : class, ICondition<TContext>
        where TContext : IConditionContext<TGeometry>
        where TGeometry : ITerrainEnvelope
    {
        private readonly ConditionVMBase<TCondition, TContext, TGeometry> target;
        private TCondition? condition;

        public ConditionToolWorker(ConditionVMBase<TCondition, TContext, TGeometry> target)
        {
            this.target = target;
            condition = target.ToDefinition();
        }

        public string ErrorMessage { get; set; } = string.Empty;

        public List<ConditionToken> Tokens { get; private set; } = new List<ConditionToken>();

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public Type ContextType => typeof(TContext);

        public void SetConditionText(string conditionText)
        {
            condition = null;
            StringSegment? error = null;
            ErrorMessage = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(conditionText))
                {
                    condition = target.Parse(conditionText);
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
                Tokens = ConditionToken.Create(conditionText, error);
            }
            catch
            {
                Tokens = new List<ConditionToken>();
            }
            if (!HasError)
            {
                target.SetParsedCondition(condition);
            }
        }

        public Task TestOnMap(ConditionTestMapViewModel tester)
        {
            return tester.TestCondition(condition as PointCondition, (target as ConditionVM)?.SamplePointProvider); // TODO !
        }

        public List<CriteriaItem> GenerateCriterias()
        {
            var category = ContextType.Name.Substring(1).Replace("ConditionContext", "");

            return ContextType.GetProperties()
                .Select(p => new CriteriaItem(category, p.Name, p.PropertyType))
                .OrderBy(p => !p.IsBoolean)
                .ThenBy(p => p.Name)
                .ToList();
        }
    }
}
