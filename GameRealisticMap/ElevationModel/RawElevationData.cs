using GeoJSON.Text.Feature;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Raw elevation data downloaded from NASA SRTM, assembled into an <see cref="ElevationGrid"/>
    /// covering the terrain area. This is the unprocessed source before lake-flattening
    /// and road-constraint solving.
    /// </summary>
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
