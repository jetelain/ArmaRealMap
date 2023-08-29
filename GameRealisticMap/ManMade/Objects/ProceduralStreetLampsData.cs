using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Objects
{
    public class ProceduralStreetLampsData : IGeoJsonData
    {
        [JsonConstructor]
        public ProceduralStreetLampsData(List<ProceduralStreetLamp> objects)
        {
            Objects = objects;
        }

        public List<ProceduralStreetLamp> Objects { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            return Objects.Select(p => new Feature(new Point(project(p.Point)), new Dictionary<string, object>() {
                {"type", "object" },
                {"object", ObjectTypeId.StreetLamp.ToString() }
            }));
        }
    }
}
