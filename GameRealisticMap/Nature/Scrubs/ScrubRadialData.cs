using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Scrubs
{
    /// <summary>
    /// Contains the polygons that form a wide ring around scrub areas (15 meters). 
    /// Used to plant smaller, low-density shrubs near scrub areas to avoid clear-cuts.
    /// </summary>
    public class ScrubRadialData : IBasicTerrainData
    {
        public const float Width = 15f;

        [JsonConstructor]
        public ScrubRadialData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "scrubRadial" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
