using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Railways
{
    internal class RailwaysData : IGeoJsonData
    {
        public RailwaysData(List<Railway> railways)
        {
            Railways = railways;
        }

        public List<Railway> Railways { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "railway" }
            };

            return Railways.Select(r => new Feature(new MultiPolygon(r.Polygons.Select(p => p.ToGeoJson())), properties));
        }
    }
}
