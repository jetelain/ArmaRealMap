using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal interface IConditionToolWorker
    {
        string ErrorMessage { get; }

        List<ConditionToken> Tokens { get; }

        bool HasError { get; }

        void SetConditionText(string conditionText);

        Task TestOnMap(ConditionTestMapViewModel tester);

        List<CriteriaItem> GenerateCriterias();

        Type ContextType { get; }
    }
}