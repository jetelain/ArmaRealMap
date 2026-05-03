using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Scrubs
{
    /// <summary>
    /// Contains scrubland and bushland polygon areas extracted from OSM (natural=scrub).
    /// Lower priority than forests; used as an intermediate ground-cover layer.
    /// </summary>
    public class ScrubData : IBasicTerrainData
    {
        [JsonConstructor]
        public ScrubData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "scrub" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
