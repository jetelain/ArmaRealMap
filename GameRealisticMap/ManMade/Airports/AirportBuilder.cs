using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Airports
{
    internal class AirportBuilder : IDataBuilder<AirportData>
    {
        private readonly IProgressSystem progress;

        public AirportBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public AirportData Build(IBuildContext context)
        {
            var clip = context.Area.TerrainBounds;

            var polygons = context.OsmSource.All
                .Where(s => s.Tags != null && s.Type != OsmSharp.OsmGeoType.Node && IsTargeted(s.Tags))
                .ProgressStep(progress, "Interpret")
                .SelectMany(s => context.OsmSource.Interpret(s))
                .SelectMany(s => TerrainPolygon.FromGeometry(s, context.Area.LatLngToTerrainPoint))
                .ProgressStep(progress, "Crop")
                .SelectMany(poly => poly.ClippedBy(clip))
                .ToList();

            return new AirportData(polygons);
        }

        private bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("aeroway") == "aerodrome" || tags.GetValue("military") == "airfield";
        }
    }
}
