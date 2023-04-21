using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Fences
{
    internal class FencesBuilder : IDataBuilder<FencesData>
    {
        private readonly IProgressSystem progress;

        public FencesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

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

        public FencesData Build(IBuildContext context)
        {
            var nodes = context.OsmSource.All
                .Where(s => s.Tags != null && s.Tags.ContainsKey("barrier"))
                .ToList();

            var fences = new List<Fence>();

            foreach (var way in nodes.ProgressStep(progress, "Paths"))
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
