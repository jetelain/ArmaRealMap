using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.Ocean
{
    internal class CoastlineBuilder : IDataBuilder<CoastlineData>
    {
        public CoastlineData Build(IBuildContext context, IProgressScope scope)
        {
            var coastlines = context.OsmSource.Ways
                .Where(w => w.Tags != null && w.Tags.GetValue("natural") == "coastline")
                .SelectMany(w => context.OsmSource.Interpret(w))
                .SelectMany(w => TerrainPath.FromGeometry(w, context.Area.LatLngToTerrainPoint))
                .Where(p => p.EnveloppeIntersects(context.Area.TerrainBounds))
                .SelectMany(p => p.ClippedBy(context.Area.TerrainBounds))
                .SelectMany(p => TerrainPolygon.FromPath(p.Points, CoastlineData.Width))
                .SelectMany(p => p.ClippedBy(context.Area.TerrainBounds))
                .ToList();

            using var report = scope.CreateInteger("Merge", coastlines.Count);
            var result = TerrainPolygon.MergeAll(coastlines, report);
            return new CoastlineData(result);
        }
    }
}
