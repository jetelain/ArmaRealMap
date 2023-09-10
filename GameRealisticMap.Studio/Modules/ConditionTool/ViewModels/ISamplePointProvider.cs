using System.Collections.Generic;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal interface ISamplePointProvider
    {
        IEnumerable<TerrainPoint> GetSamplePoints(IBuildContext buildContext);
    }
}
