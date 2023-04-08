using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.WaterWays
{
    public class WaterWaysData : IBasicTerrainData
    {
        public WaterWaysData(List<WaterWay> waterwayPaths, List<TerrainPolygon> polygons)
        {
            WaterwayPaths = waterwayPaths;
            Polygons = polygons;
        }

        public List<WaterWay> WaterwayPaths { get; }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "waterwaySurface" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
