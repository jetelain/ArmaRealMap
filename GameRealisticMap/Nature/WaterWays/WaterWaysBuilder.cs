using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GeoAPI.Geometries;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.WaterWays
{
    internal class WaterWaysBuilder : IDataBuilder<WaterWaysData>
    {
        private readonly IProgressSystem progress;

        public WaterWaysBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        private static bool IsWaterWay(TagsCollectionBase tags)
        {
            return (tags.ContainsKey("waterway") && !tags.IsFalse("waterway") || tags.GetValue("water") == "stream")
                && !tags.ContainsKey("tunnel");
        }

        public WaterWaysData Build(IBuildContext context)
        {
            var roadsData = context.GetData<RoadsData>();
            var lakesPolygons = context.GetData<LakesData>().Polygons;

            var waterWaysPaths = context.OsmSource.All
                .Where(s => s.Tags != null && IsWaterWay(s.Tags)) // XXX: Compute width ? (from type)

                .ProgressStep(progress, "Interpret")
                .SelectMany(s => context.OsmSource.Interpret(s))
                .OfType<ILineString>() // XXX: Include IMultiLineString ?
                .Where(l => !l.IsClosed)
                .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))

                .ProgressStep(progress, "Crop")
                .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds))

                .ProgressStep(progress, "Lakes")
                .SelectMany(p => p.SubstractAll(lakesPolygons))
                .Where(w => w.Length > 10f) // XXX: Merge ? (before the length filter)
                .ToList();

            return new WaterWaysData(waterWaysPaths);
        }
    }
}
