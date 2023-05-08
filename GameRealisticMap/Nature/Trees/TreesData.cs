using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Trees
{
    public class TreesData : IGeoJsonData
    {
        [JsonConstructor]
        public TreesData(List<TerrainPoint> points)
        {
            Points = points;
        }

        public List<TerrainPoint> Points { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "tree" }
            };

            return Points.Select(p => new Feature(new Point(p), properties));
        }
    }
}
