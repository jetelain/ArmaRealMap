using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Surfaces
{
    public class IceSurfaceData : IBasicTerrainData
    {
        [JsonConstructor]
        public IceSurfaceData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "ice" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
