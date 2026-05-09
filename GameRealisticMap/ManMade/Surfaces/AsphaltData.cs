using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Surfaces
{
    /// <summary>
    /// Contains paved surface area polygons (parking lots, plazas, service areas)
    /// extracted from OSM (amenity=parking, highway=pedestrian with area=yes, etc.).
    /// </summary>
    public class AsphaltData : IBasicTerrainData
    {
        [JsonConstructor]
        public AsphaltData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "asphalt" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
