using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using MapToolkit.Contours;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationData : IGeoJsonData
    {

        public ElevationData(ElevationGrid elevation, IEnumerable<LineString> geoJSON)
        {
            Elevation = elevation;
            this.geoJSON = geoJSON;
        }

        public ElevationGrid Elevation { get; }

        private readonly IEnumerable<LineString> geoJSON;

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "contour");

            return geoJSON.Select(g => new Feature(g, properties));
        }
    }
}
