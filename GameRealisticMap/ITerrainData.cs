using GeoJSON.Text.Feature;

namespace GameRealisticMap
{
    public interface ITerrainData
    {
        IEnumerable<Feature> ToGeoJson();
    }
}