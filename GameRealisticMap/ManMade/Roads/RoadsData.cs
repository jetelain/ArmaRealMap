using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Roads
{
    public class RoadsData : IGeoJsonData
    {
        [JsonConstructor]
        public RoadsData(List<Road> roads)
        { 
            Roads = roads;
        }

        public List<Road> Roads { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            return Roads.Select(r => new Feature(new MultiPolygon(r.Polygons.Select(p => p.ToGeoJson(project))), new Dictionary<string, object>() {
                {"type", "road" },
                {"road", r.RoadType.ToString() }
            }));
        }
    }
}
