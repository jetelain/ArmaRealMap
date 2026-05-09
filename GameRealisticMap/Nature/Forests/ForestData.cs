using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Forests
{
    /// <summary>
    /// Contains forest and woodland polygon areas extracted from OSM
    /// (landuse=forest, natural=wood). Has higher priority than scrub and other surface types.
    /// </summary>
    public class ForestData : IBasicTerrainData
    {
        [JsonConstructor]
        public ForestData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "forest" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
