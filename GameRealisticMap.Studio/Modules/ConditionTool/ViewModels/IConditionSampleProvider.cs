using System.Collections.Generic;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal interface IConditionSampleProvider<TGeometry> where TGeometry : ITerrainEnvelope
    {
        IEnumerable<TGeometry> GetSamplePoints(IBuildContext buildContext);
    }
}
