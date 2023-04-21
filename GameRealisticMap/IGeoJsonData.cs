using GeoJSON.Text.Feature;

namespace GameRealisticMap
{
    public interface IGeoJsonData
    {
        IEnumerable<Feature> ToGeoJson();
    }
}