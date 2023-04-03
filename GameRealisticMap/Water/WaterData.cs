using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Water
{
    public class WaterData : ITerrainData
    {
        public WaterData(List<TerrainPolygon> lakesPolygons, List<TerrainPath> waterWaysPaths)
        {
            LakesPolygons = lakesPolygons;
            WaterWaysPaths = waterWaysPaths;
        }

        public List<TerrainPolygon> LakesPolygons { get; }

        public List<TerrainPath> WaterWaysPaths { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "lake" }
            };
            return LakesPolygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
