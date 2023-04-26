using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Railways
{
    internal class RailwaysBuilder : IDataBuilder<RailwaysData>
    {
        private readonly IProgressSystem progress;

        public RailwaysBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public RailwaysData Build(IBuildContext context)
        {
            var nodes = context.OsmSource.All
                .Where(s => s.Tags != null && s.Tags.GetValue("railway") == "rail")
                .ToList();

            var fences = new List<Railway>();
            foreach (var way in nodes.ProgressStep(progress, "Paths"))
            {
                foreach (var segment in context.OsmSource.Interpret(way)
                                                .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                                                .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds)))
                {
                    fences.Add(new Railway(WaySpecialSegmentHelper.FromOSM(way.Tags), segment));
                }
            }

            return new RailwaysData(fences);
        }
    }
}
