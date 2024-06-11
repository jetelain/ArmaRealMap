namespace GameRealisticMap
{
    public class MapProcessingOptions : IMapProcessingOptions
    {
        public MapProcessingOptions(double resolution = 1, float privateServiceRoadThreshold = 25)
        {
            Resolution = resolution;
            PrivateServiceRoadThreshold = privateServiceRoadThreshold;
        }

        public double Resolution { get; }

        public float PrivateServiceRoadThreshold { get; }
    }
}
