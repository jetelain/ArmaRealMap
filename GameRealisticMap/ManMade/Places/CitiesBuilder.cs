using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp;
using OsmSharp.Geo;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Places
{
    internal class CitiesBuilder : IDataBuilder<CitiesData>
    {
        private readonly IProgressSystem progress;

        public CitiesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public CitiesData Build(IBuildContext context)
        {
            var result = new List<City>();
            var boundaries = context.OsmSource.Relations.Where(r => r.Tags != null && r.Tags.GetValue("boundary") == "administrative").ToList();
            foreach (var node in context.OsmSource.Nodes.Where(n => n.Tags != null && n.Tags.ContainsKey("place")).ProgressStep(progress, "Cities"))
            {
                var typeId = GetCityTypeId(node.Tags.GetValue("place"));
                if (typeId != null)
                {
                    var radius = 0f;
                    var boundaryPolygon = new List<TerrainPolygon>();
                    var center = context.Area.LatLngToTerrainPoint(node.GetCoordinate());
                    var tags = new TagsCollection(node.Tags);

                    var boundary = boundaries.Where(r => r.Members.Any(m => m.Id == node.Id)).FirstOrDefault();
                    if (boundary != null)
                    {
                        tags.AddOrReplace(boundary.Tags);

                        radius = GetRadius(context, center, boundary);

                        boundaryPolygon.AddRange(context.OsmSource.Interpret(boundary)
                            .SelectMany(g => TerrainPolygon.FromGeometry(g, context.Area.LatLngToTerrainPoint))
                            .SelectMany(p => p.Intersection(context.Area.TerrainBounds)));
                    }

                    var name = tags.GetValue("name");
                    var population = GetNumber(tags.GetValue("population"));

                    result.Add(new City(center, boundaryPolygon, name, typeId.Value, radius, population));
                }
            }
            return new CitiesData(result);
        }

        private float GetNumber(string value)
        {
            if ( !string.IsNullOrEmpty(value) && float.TryParse(value, out var population))
            {
                return population; 
            } 
            return 0;
        }

        private static float GetRadius(IBuildContext context, TerrainPoint center, Relation boundary)
        {
            var points = boundary.Members
                .Where(m => m.Role == "outer")
                .Select(m => context.OsmSource.SnapshotDb.Get(m.Type, m.Id))
                .OfType<Way>()
                .SelectMany(w => w.Nodes)
                .Select(m => context.OsmSource.SnapshotDb.Get(OsmGeoType.Node, m))
                .OfType<Node>()
                .Select(m => context.Area.LatLngToTerrainPoint(m.GetCoordinate()))
                .ToList();

            return MathF.Sqrt(points.Min(p => (p.Vector - center.Vector).LengthSquared()));
        }

        private CityTypeId? GetCityTypeId(string place)
        {
            switch (place)
            {
                case "city":
                    return CityTypeId.City;

                case "town":
                    return CityTypeId.Town;

                case "village":
                    return CityTypeId.Village;

                case "hamlet":
                    return CityTypeId.Hamlet;
            }
            return null;
        }
    }
}
