using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class RawElevationData : ITerrainData
    {
        public RawElevationData(ElevationGrid rawElevation)
        {
            RawElevation = rawElevation;
        }

        public ElevationGrid RawElevation { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Enumerable.Empty<Feature>();
        }
    }
}
