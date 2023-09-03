using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Roads
{
    public class SidewalksData : IGeoJsonData
    {
        [JsonConstructor]
        public SidewalksData(List<TerrainPath> paths)
        {
            Paths = paths;
        }

        public List<TerrainPath> Paths { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "sidewalk" }
            };
            return Paths.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
