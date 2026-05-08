using GameRealisticMap.Configuration;

namespace GameRealisticMap
{
    /// <summary>
    /// Default implementation of <see cref="IMapProcessingOptions"/> with sensible defaults
    /// suitable for most standard-resolution map generation.
    /// Use <see cref="Default"/> for a shared default instance.
    /// </summary>
    public class MapProcessingOptions : IMapProcessingOptions
    {
        /// <summary>A shared default instance with 1 m/px resolution and a 25 m service-road threshold.</summary>
        public static IMapProcessingOptions Default { get; } = new MapProcessingOptions();

        /// <summary>
        /// Initialises processing options.
        /// </summary>
        /// <param name="resolution">Imagery resolution in metres per pixel (default 1.0).</param>
        /// <param name="privateServiceRoadThreshold">Service roads shorter than this (in metres) are ignored (default 25).</param>
        public MapProcessingOptions(double resolution = 1, float privateServiceRoadThreshold = 25)
        {
            Resolution = resolution;
            PrivateServiceRoadThreshold = privateServiceRoadThreshold;
        }

        public double Resolution { get; }

        public float PrivateServiceRoadThreshold { get; }

        public ISatelliteImageOptions Satellite => new SatelliteImageOptions();
    }
}
