using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal sealed class RandomSampleProvider : IConditionSampleProvider<TerrainPoint>, IConditionSampleProvider<TerrainPath>, IConditionSampleProvider<TerrainPolygon>
    {
        IEnumerable<TerrainPoint> IConditionSampleProvider<TerrainPoint>.GetSamplePoints(IBuildContext buildContext)
        {
            var count = (int)Math.Min((buildContext.Area.SizeInMeters / 100) * (buildContext.Area.SizeInMeters / 100), 50_000);
            var size = (int)(buildContext.Area.SizeInMeters * 10);

            return Enumerable.Range(0, count).Select(_ => new TerrainPoint(Random.Shared.Next(0, size) / 10f, Random.Shared.Next(0, size) / 10f)).ToList();
        }

        IEnumerable<TerrainPath> IConditionSampleProvider<TerrainPath>.GetSamplePoints(IBuildContext buildContext)
        {
            var count = (int)Math.Min((buildContext.Area.SizeInMeters / 250) * (buildContext.Area.SizeInMeters / 250), 20_000);
            var size = (int)(buildContext.Area.SizeInMeters * 10);

            return Enumerable.Range(0, count)
                .Select(_ => new TerrainPoint(Random.Shared.Next(0, size) / 10f, Random.Shared.Next(0, size) / 10f))
                .Select((p, i) => new TerrainPath(p, p + ((i % 2 == 1) ? new Vector2(50, 50) : new Vector2(-50, 50))))
                .ToList();
        }

        IEnumerable<TerrainPolygon> IConditionSampleProvider<TerrainPolygon>.GetSamplePoints(IBuildContext buildContext)
        {
            var count = (int)Math.Min((buildContext.Area.SizeInMeters / 250) * (buildContext.Area.SizeInMeters / 250), 20_000);
            var size = (int)(buildContext.Area.SizeInMeters * 10);

            return Enumerable.Range(0, count)
                .Select(_ => new TerrainPoint(Random.Shared.Next(0, size) / 10f, Random.Shared.Next(0, size) / 10f))
                .Select(p => TerrainPolygon.FromRectangle(p, p + new Vector2(50, 50)))
                .ToList();
        }
    }
}
