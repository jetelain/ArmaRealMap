using GameRealisticMap.Geometries;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature
{
    internal abstract class PolygonBuilderBase
    {
        protected abstract bool IsTargeted(TagsCollectionBase tags);

        protected abstract IEnumerable<TerrainPolygon> GetPriority(IBuildContext context);

        internal List<TerrainPolygon> GetPolygons(IBuildContext context, IEnumerable<TerrainPolygon> additionals, IProgressScope scope)
        {
            var priority = GetPriority(context).ToList();

            var clip = GetClipArea(context);

            var polygons = context.OsmSource.All
                .Where(s => s.Tags != null && IsTargeted(s.Tags))

                .WithProgress(scope, "Interpret")
                .SelectMany(s => context.OsmSource.Interpret(s))
                .SelectMany(s => TerrainPolygon.FromGeometry(s, context.Area.LatLngToTerrainPoint))
                .Concat(additionals)

                .WithProgress(scope, "Crop")
                .SelectMany(poly => poly.ClippedBy(clip))

                .RemoveOverlaps(scope, "Overlaps")

                .SubstractAll(scope, "Priority", priority)
                .ToList();

            return MergeIfRequired(polygons, scope);
        }

        protected virtual TerrainPolygon GetClipArea(IBuildContext context)
        {
            return context.Area.TerrainBounds;
        }

        protected virtual List<TerrainPolygon> MergeIfRequired(List<TerrainPolygon> polygons, IProgressScope scope)
        {
#if PARALLEL
            using (scope.CreateInteger("Merge (Parallel)", polygons.Count))
            {
                return TerrainPolygon.MergeAllParallel(polygons);
            }
#else
            using (var step = scope.CreateInteger("Merge (Plain)", polygons.Count))
            {
                return TerrainPolygon.MergeAll(polygons, step);
            }
#endif
        }
    }
}
