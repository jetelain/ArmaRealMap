using GameRealisticMap.Geometries;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Fences
{
    internal class FencesBuilder : IDataBuilder<FencesData>
    {
        private static FenceTypeId? GetFenceTypeId(TagsCollectionBase tags)
        {
            if (tags.TryGetValue("barrier", out var barrier))
            {
                switch(barrier)
                {
                    case "wall": 
                        return FenceTypeId.Wall;
                    case "fence":
                        return FenceTypeId.Fence;
                    case "hedge": 
                        return FenceTypeId.Hedge;
                }
            }
            return null;
        }

        public FencesData Build(IBuildContext context, IProgressScope scope)
        {
            var nodes = context.OsmSource.All
                .Where(s => s.Tags != null && s.Tags.ContainsKey("barrier"))
                .ToList();

            var fences = new List<Fence>();

            foreach (var way in nodes.WithProgress(scope, "Paths"))
            {
                var kind = GetFenceTypeId(way.Tags);
                if (kind != null)
                {
                    foreach (var segment in context.OsmSource.Interpret(way)
                                                    .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                                                    .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds)))
                    {
                        fences.Add(new Fence(segment, kind.Value));
                    }
                }
            }
            return new FencesData(fences);
        }
    }
}
