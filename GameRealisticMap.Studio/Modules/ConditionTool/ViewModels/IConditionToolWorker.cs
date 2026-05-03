using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    /// <summary>
    /// Performs the actual condition parsing, token highlighting, and map-test execution
    /// in the condition editor tool. Decouples the parsing logic from the view-model
    /// so it can be varied per geometry type (point / path / polygon).
    /// </summary>
    internal interface IConditionToolWorker
    {
        string ErrorMessage { get; }

        List<ConditionToken> Tokens { get; }

        bool HasError { get; }

        void SetConditionText(string conditionText);

        Task TestOnMap(ConditionTestMapViewModel tester);

        List<CriteriaItem> GenerateCriterias();
        
        Task TestOnMapViewport(ConditionTestMapViewModel conditionTestMapViewModel, ITerrainEnvelope envelope);

        Task TestOnMapRandom(ConditionTestMapViewModel conditionTestMapViewModel);

        Type ContextType { get; }
    }
}