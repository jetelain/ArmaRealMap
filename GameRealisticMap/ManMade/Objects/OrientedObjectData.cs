using System.Text.Json.Serialization;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Objects
{
    public class OrientedObjectData : IGeoJsonData
    {
        [JsonConstructor]
        public OrientedObjectData(List<OrientedObject> objects)
        {
            Objects = objects;
        }

        public List<OrientedObject> Objects { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Objects.Select(p => new Feature(new Point(p.Point), new Dictionary<string, object>() {
                {"type", "object" },
                {"object", p.TypeId.ToString() }
            }));
        }
    }
}
