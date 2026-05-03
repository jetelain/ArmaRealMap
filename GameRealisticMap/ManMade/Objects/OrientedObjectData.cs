using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Objects
{
    /// <summary>
    /// Contains point objects with a defined orientation extracted from OSM nodes
    /// (power poles, wind turbines, water towers, pylons, etc.).
    /// Each object carries its position and heading so the correct facing model can be placed.
    /// </summary>
    public class OrientedObjectData : IGeoJsonData
    {
        [JsonConstructor]
        public OrientedObjectData(List<OrientedObject> objects)
        {
            Objects = objects;
        }

        public List<OrientedObject> Objects { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            return Objects.Select(p => new Feature(new Point(project(p.Point)), new Dictionary<string, object>() {
                {"type", "object" },
                {"object", p.TypeId.ToString() }
            }));
        }
    }
}
