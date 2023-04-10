using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationWithLakesData : ITerrainData
    {

        public ElevationWithLakesData(ElevationGrid elevation, List<LakeWithElevation> lakes)
        {
            Elevation = elevation;
            Lakes = lakes;
        }

        public ElevationGrid Elevation { get; }

        public List<LakeWithElevation> Lakes { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "realLake");
            return Lakes.Select(l => new Feature(l.TerrainPolygon.ToGeoJson(), properties));
        }
    }
}
