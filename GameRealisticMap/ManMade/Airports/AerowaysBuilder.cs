using System.Globalization;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Airports
{
    internal class AerowaysBuilder : IDataBuilder<AerowaysData>
    {
        private readonly IProgressSystem progress;

        public AerowaysBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public AerowaysData Build(IBuildContext context)
        {
            var airports = context.GetData<AirportData>().Polygons;
            var outsideAirports = new List<Aeroway>();
            var insideAirports = new Dictionary<TerrainPolygon, AirportAeroways>(ReferenceEqualityComparer.Instance);

            foreach(var way in context.OsmSource.Ways
                .Where(w => w.Tags != null && w.Tags.ContainsKey("aeroway"))
                .ProgressStep(progress,"Aeroways"))
            {
                var type = GetAerowayType(way.Tags);
                if (type != null)
                {
                    foreach (var segment in context.OsmSource.Interpret(way)
                        .SelectMany(g => TerrainPath.FromGeometry(g, context.Area.LatLngToTerrainPoint))
                        .SelectMany(p => p.ClippedKeepOrientation(context.Area.TerrainBounds)))
                    {
                        var aeroway = new Aeroway(segment, type.Value, width: GetWidth(type.Value, way.Tags));
                        var airport = airports.FirstOrDefault(a => a.Contains(segment));
                        if (airport != null)
                        {
                            if (!insideAirports.TryGetValue(airport, out var apWays))
                            {
                                insideAirports.Add(airport, apWays = new AirportAeroways(airport, new List<Aeroway>()));
                            }
                            apWays.Aeroways.Add(aeroway);
                        }
                        else
                        {
                            outsideAirports.Add(aeroway);
                        }
                    }
                }
            }

            return new AerowaysData(insideAirports.Values.ToList(), outsideAirports);
        }

        private float GetWidth(AerowayTypeId type, TagsCollectionBase tags)
        {
            var width = tags.GetValue("width");
            if (!string.IsNullOrEmpty(width) && float.TryParse(width, NumberStyles.Number, CultureInfo.InvariantCulture, out var widthValue))
            {
                return widthValue;
            }
            switch(type)
            {
                case AerowayTypeId.Taxiway:
                    return 30;

                case AerowayTypeId.Runway:
                default:
                    return 60;
            }
        }

        private AerowayTypeId? GetAerowayType(TagsCollectionBase tags)
        {
            switch(tags.GetValue("aeroway"))
            {
                case "runway":
                    return AerowayTypeId.Runway;

                case "taxiway":
                    return AerowayTypeId.Taxiway;
            }
            return null;
        }
    }
}
