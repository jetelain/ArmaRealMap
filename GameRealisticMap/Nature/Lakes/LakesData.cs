using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Lakes
{
    /// <summary>
    /// Contains lake and pond water-body polygon areas extracted from OSM (natural=water).
    /// Used as priority polygons (no forest/scrub inside lakes) and as input for
    /// elevation flattening in <see cref="GameRealisticMap.ElevationModel.ElevationWithLakesBuilder"/>.
    /// </summary>
    public class LakesData : IBasicTerrainData
    {
        [JsonConstructor]
        public LakesData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "lake" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
