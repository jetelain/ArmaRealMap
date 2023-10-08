using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    public class RawElevationData
    {
        public RawElevationData(ElevationGrid rawElevation)
        {
            RawElevation = rawElevation;
            Credits = new ();
            OutOfBounds = new ElevationMinMax[0];
        }

        public RawElevationData(ElevationGrid rawElevation, List<string> credits, ElevationMinMax[] outOfBounds)
        {
            RawElevation = rawElevation;
            Credits = credits;
            OutOfBounds = outOfBounds;
        }

        public ElevationGrid RawElevation { get; }

        public List<string> Credits { get; }

        internal ElevationMinMax[] OutOfBounds { get; }
    }
}
