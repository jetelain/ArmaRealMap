using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class RawElevationData
    {
        public RawElevationData(ElevationGrid rawElevation)
        {
            RawElevation = rawElevation;
            Credits = new ();
        }

        public RawElevationData(ElevationGrid rawElevation, List<string> credits)
        {
            RawElevation = rawElevation;
            Credits = credits;
        }

        public ElevationGrid RawElevation { get; }
        public List<string> Credits { get; }
    }
}
