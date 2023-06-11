using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Watercourses
{
    public class WatercoursesData : IBasicTerrainData
    {
        [JsonConstructor]
        public WatercoursesData(List<Watercourse> waterwayPaths, List<TerrainPolygon> polygons)
        {
            Paths = waterwayPaths;
            Polygons = polygons;
        }

        public List<Watercourse> Paths { get; }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "watercourseSurface" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
