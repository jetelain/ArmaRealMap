using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Watercourses
{
    public class WatercoursesData : IBasicTerrainData
    {
        public WatercoursesData(List<Watercourse> waterwayPaths, List<TerrainPolygon> polygons)
        {
            Paths = waterwayPaths;
            Polygons = polygons;
        }

        public List<Watercourse> Paths { get; }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "watercourseSurface" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
