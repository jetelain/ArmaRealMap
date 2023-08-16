using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class VineyardData : IBasicTerrainData
    {
        [JsonConstructor]
        public VineyardData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "vineyard" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
