using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.ManMade.Farmlands
{
    public class FarmlandsData : IBasicTerrainData
    {
        [JsonConstructor]
        public FarmlandsData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "farmland" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
