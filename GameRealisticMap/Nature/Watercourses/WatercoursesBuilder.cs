using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GeoAPI.Geometries;
using OsmSharp;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Watercourses
{
    internal class WatercoursesBuilder : IDataBuilder<WatercoursesData>
    {
        private readonly IProgressSystem progress;

        public WatercoursesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        private static WatercourseTypeId? GetWaterwayPathTypeId(TagsCollectionBase tags)
        {
            if (tags.TryGetValue("waterway", out var waterway))
            {
                if (tags.ContainsKey("tunnel"))
                {
                    switch (waterway)
                    {
                        case "river":
                            return WatercourseTypeId.RiverTunnel;

                        case "stream":
                            return WatercourseTypeId.StreamTunnel;
                    }
                }
                else
                {
                    switch (waterway)
                    {
                        case "river":
                            return WatercourseTypeId.River;

                        case "stream":
                            return WatercourseTypeId.Stream;
                    }
                }
            }
            return null;
        }

        private static bool IsWaterwaySurface(TagsCollectionBase tags)
        {
            if (tags.TryGetValue("water", out var water))
            {
                switch (water)
                {
                    case "river":
                    case "stream":
                        return true;
                }
            }
            return false;
        }

        public WatercoursesData Build(IBuildContext context)
        {
            var lakesPolygons = context.GetData<LakesData>().Polygons; // XXX: With elevation or not ?

            var waterwayNodes = context.OsmSource.All
                .Where(s => s.Tags != null && GetWaterwayPathTypeId(s.Tags) != null)
                .ToList();

            var waterwaysPaths = GetPaths(context, lakesPolygons, waterwayNodes);

            var polygons = GetSurface(context, lakesPolygons, waterwaysPaths);

            return new WatercoursesData(waterwaysPaths, polygons);
        }

        private List<TerrainPolygon> GetSurface(IBuildContext context, List<TerrainPolygon> lakesPolygons, List<Watercourse> waterwaysPaths)
        {
            var priority = lakesPolygons
                .Concat(context.GetData<RoadsData>().Roads.Where(r => r.RoadType != RoadTypeId.Trail && r.SpecialSegment != WaySpecialSegment.Bridge).SelectMany(r => r.ClearPolygons))
                .ToList();

            var builder = new PolygonBuilder(progress, IsWaterwaySurface, priority);

            var surfaceOfWays = waterwaysPaths.Where(w => !w.IsTunnel).SelectMany(w => w.Polygons);

            return TerrainPolygon.MergeAll(builder.GetPolygons(context, surfaceOfWays));
        }

        private List<Watercourse> GetPaths(IBuildContext context, List<TerrainPolygon> lakesPolygons, List<OsmGeo> waterwayNodes)
        {
            var waterwaysPaths = new List<Watercourse>();
            using (var report = progress.CreateStep("Paths", waterwayNodes.Count))
            {
                foreach (var way in waterwayNodes)
                {
                    var kind = GetWaterwayPathTypeId(way.Tags);
                    if (kind != null)
                    {
                        foreach (var segment in context.OsmSource.Interpret(way)
                                                        .OfType<ILineString>()
                                                        .Where(l => !l.IsClosed)
                                                        .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                                                        .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds))
                                                        .SelectMany(p => p.SubstractAll(lakesPolygons)))
                        {
                            waterwaysPaths.Add(new Watercourse(segment, kind.Value));
                        }
                    }
                    report.ReportOneDone();
                }
            }
            return waterwaysPaths;
        }
    }
}
