using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestEdgeData : IGeoJsonData
    {
        public ForestEdgeData(List<TerrainPolygon> edges, List<TerrainPolygon> mergedForests)
        {
            Edges = edges;
            MergedForests = mergedForests;
        }

        public List<TerrainPolygon> Edges { get; }
        public List<TerrainPolygon> MergedForests { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "forestEdge" }
            };
            var properties2 = new Dictionary<string, object>() {
                {"type", "forestMerged" }
            };
            return Edges.Select(b => new Feature(b.ToGeoJson(), properties)).Concat(MergedForests.Select(b => new Feature(b.ToGeoJson(), properties2)));
        }
    }
}
