using GeoJSON.Text.Feature;

namespace GameRealisticMap.ManMade.Fences
{
    public class FencesData : IGeoJsonData
    {
        public FencesData(List<Fence> fences)
        {
            Fences = fences;
        }

        public List<Fence> Fences { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "fence" }
            };
            return Fences.Select(b => new Feature(b.Path.ToGeoJson(), properties));
        }
    }
}
