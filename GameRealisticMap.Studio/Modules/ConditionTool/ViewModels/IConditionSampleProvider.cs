using System.Collections.Generic;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    /// <summary>
    /// Supplies sample geometry items from a build context for testing a condition
    /// expression. Implemented separately for random sampling and viewport-bounded sampling.
    /// </summary>
    internal interface IConditionSampleProvider<TGeometry> where TGeometry : ITerrainEnvelope
    {
        IEnumerable<TGeometry> GetSamplePoints(IBuildContext buildContext);
    }
}
