using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class RawElevationData
    {
        public RawElevationData(ElevationGrid rawElevation)
        {
            RawElevation = rawElevation;
        }

        public ElevationGrid RawElevation { get; }
    }
}
