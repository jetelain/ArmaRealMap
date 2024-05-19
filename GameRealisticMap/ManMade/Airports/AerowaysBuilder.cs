using System.Globalization;
using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using NetTopologySuite.Utilities;
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
                        Aeroway aeroway;
                        if (segment.IsClosed)
                        {
                            // Probably invalid mapping: it's a surface, use a bounding box
                            var box = BoundingBox.Compute(segment.Points.ToArray());
                            var matrix = Matrix3x2.CreateRotation((float)(Math.PI * box.Angle / 180));
                            if (box.Width > box.Height)
                            {
                                var delta = Vector2.Transform(new Vector2(box.Width / 2, 0), matrix);
                                aeroway = new Aeroway(new TerrainPath(box.Center + delta, box.Center - delta), type.Value, width: box.Height, surface: GetSurface(way.Tags));
                            }
                            else
                            {
                                var delta = Vector2.Transform(new Vector2(0, box.Height / 2), matrix);
                                aeroway = new Aeroway(new TerrainPath(box.Center + delta, box.Center - delta), type.Value, width: box.Width, surface: GetSurface(way.Tags));
                            }
                        }
                        else
                        {
                            aeroway = new Aeroway(segment, type.Value, width: GetWidth(type.Value, way.Tags), surface: GetSurface(way.Tags));
                        }
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

        private AerowaySurface GetSurface(TagsCollectionBase tags)
        {
            switch(tags.GetValue("surface"))
            {
                case "grass": 
                    return AerowaySurface.Grass;

                case "asphalt": 
                    return AerowaySurface.Asphalt;
            }
            return AerowaySurface.Default;
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

        internal static AerowayTypeId? GetAerowayType(TagsCollectionBase tags)
        {
            switch(tags.GetValue("aeroway"))
            {
                case "runway":
                    return AerowayTypeId.Runway;

                case "taxiway":
                case "taxilane":
                    return AerowayTypeId.Taxiway;
            }
            return null;
        }
    }
}
