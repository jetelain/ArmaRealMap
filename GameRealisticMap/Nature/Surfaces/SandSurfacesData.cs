using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Surfaces
{
    /// <summary>
    /// Contains sandy surface polygon areas extracted from OSM
    /// (natural=sand, natural=beach, natural=dune).
    /// </summary>
    public class SandSurfacesData : IBasicTerrainData
    {
        [JsonConstructor]
        public SandSurfacesData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "sand" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
