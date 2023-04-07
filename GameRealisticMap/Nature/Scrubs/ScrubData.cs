using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Scrubs
{
    public class ScrubData : IBasicTerrainData
    {
        public ScrubData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "scrub" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
