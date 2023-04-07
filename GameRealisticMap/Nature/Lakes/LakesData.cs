using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Lakes
{
    public class LakesData : IBasicTerrainData
    {
        public LakesData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "lake" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
