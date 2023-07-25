using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Railways
{
    public class RailwaysData : IGeoJsonData
    {
        [JsonConstructor]
        public RailwaysData(List<Railway> railways)
        {
            Railways = railways;
        }

        public List<Railway> Railways { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "railway" }
            };

            return Railways.Select(r => new Feature(new MultiPolygon(r.Polygons.Select(p => p.ToGeoJson(project))), properties));
        }
    }
}
