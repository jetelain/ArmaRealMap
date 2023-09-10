using System;
using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal class RandomPointProvider : ISamplePointProvider
    {
        public IEnumerable<TerrainPoint> GetSamplePoints(IBuildContext buildContext)
        {
            var count = (int)Math.Min((buildContext.Area.SizeInMeters / 100) * (buildContext.Area.SizeInMeters / 100), 50_000);
            var size = (int)(buildContext.Area.SizeInMeters * 10);
            return Enumerable.Range(0, count).Select(_ => new TerrainPoint(Random.Shared.Next(0, size) / 10f, Random.Shared.Next(0, size) / 10f)).ToList();
        }
    }
}
