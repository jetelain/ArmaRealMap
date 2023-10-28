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

            var waysByAirport = new Dictionary<TerrainPolygon, AirportsAeroways>(ReferenceEqualityComparer.Instance);

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
                        var airport = airports.FirstOrDefault(a => a.Contains(segment));
                        if (airport != null)
                        {
                            if (!waysByAirport.TryGetValue(airport, out var apWays))
                            {
                                waysByAirport.Add(airport, apWays = new AirportsAeroways(airport, new List<Aeroway>()));
                            }
                            apWays.Aeroways.Add(new Aeroway(segment, type.Value, width: GetWidth(type.Value, way.Tags)));
                        }
                    }
                }
            }

            return new AerowaysData(waysByAirport.Values.ToList());
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
            }
            return null;
        }
    }
}
