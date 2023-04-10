using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using MapToolkit.Contours;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationData : ITerrainData
    {

        public ElevationData(ElevationGrid elevation, List<LakeWithElevation> lakes, IEnumerable<LineString> geoJSON)
        {
            Elevation = elevation;
            Lakes = lakes;
            this.geoJSON = geoJSON;
        }

        public ElevationGrid Elevation { get; }

        public List<LakeWithElevation> Lakes { get; }


        private readonly IEnumerable<LineString> geoJSON;

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "contour");
            return geoJSON.Select(g => new Feature(g, properties));
        }
    }
}
