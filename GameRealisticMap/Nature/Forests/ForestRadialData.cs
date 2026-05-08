using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Forests
{
    /// <summary>
    /// Contains the polygons that form a wide ring around forested areas (25 meters). 
    /// Used to plant smaller, low-density trees near forests to avoid clear-cuts.
    /// </summary>
    public class ForestRadialData : IBasicTerrainData
    {
        public const float Width = 25f;

        [JsonConstructor]
        public ForestRadialData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "forestRadial" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
