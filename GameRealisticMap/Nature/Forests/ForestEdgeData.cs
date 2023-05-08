using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Forests
{
    public class ForestEdgeData : IBasicTerrainData
    {
        [JsonConstructor]
        public ForestEdgeData(List<TerrainPolygon> edges, List<TerrainPolygon> mergedForests)
        {
            Polygons = edges;
            MergedForests = mergedForests;
        }

        public List<TerrainPolygon> Polygons { get; }

        public List<TerrainPolygon> MergedForests { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "forestEdge" }
            };
            var properties2 = new Dictionary<string, object>() {
                {"type", "forestMerged" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties)).Concat(MergedForests.Select(b => new Feature(b.ToGeoJson(), properties2)));
        }
    }
}
