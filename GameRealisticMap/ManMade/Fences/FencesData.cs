using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Fences
{
    /// <summary>
    /// Contains fence, wall, and hedgerow linear features extracted from OSM
    /// (barrier=fence, barrier=wall, barrier=hedge, etc.).
    /// </summary>
    public class FencesData : IGeoJsonData
    {
        [JsonConstructor]
        public FencesData(List<Fence> fences)
        {
            Fences = fences;
        }

        public List<Fence> Fences { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "fence" }
            };
            return Fences.Select(b => new Feature(b.Path.ToGeoJson(project), properties));
        }
    }
}
