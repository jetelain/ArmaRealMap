using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationData : ITerrainData
    {

        public ElevationData(ElevationGrid elevation, List<LakeWithElevation> lakes)
        {
            Elevation = elevation;
            Lakes = lakes;
        }

        public ElevationGrid Elevation { get; }
        public List<LakeWithElevation> Lakes { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Enumerable.Empty<Feature>();
        }
    }
}
