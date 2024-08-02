using GameRealisticMap.Geometries;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Airports
{
    internal class AirportBuilder : IDataBuilder<AirportData>
    {

        public AirportData Build(IBuildContext context, IProgressScope scope)
        {
            var clip = context.Area.TerrainBounds;

            var polygons = context.OsmSource.All
                .Where(s => s.Tags != null && s.Type != OsmSharp.OsmGeoType.Node && IsTargeted(s.Tags))
                .WithProgress(scope, "Interpret")
                .SelectMany(s => context.OsmSource.Interpret(s))
                .SelectMany(s => TerrainPolygon.FromGeometry(s, context.Area.LatLngToTerrainPoint))
                .WithProgress(scope, "Crop")
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
