using GameRealisticMap.Configuration;

namespace GameRealisticMap
{
    /// <summary>
    /// Options that control how map data is processed and at what quality or detail level.
    /// Passed to all builders via <see cref="IBuildContext.Options"/>.
    /// The Arma 3 implementation is <see cref="GameRealisticMap.Arma3.Arma3MapConfig"/>.
    /// A default implementation is available as <see cref="MapProcessingOptions.Default"/>.
    /// </summary>
    public interface IMapProcessingOptions
    {
        /// <summary>
        /// The imagery resolution in metres per pixel used when generating satellite
        /// and material textures. Default is <c>1.0</c> (one pixel per metre).
        /// </summary>
        double Resolution { get; }

        /// <summary>
        /// Maximum road width in metres below which a service road is classified as a
        /// private access road and excluded from the main routable road network.
        /// Roads narrower than this value are rendered differently or omitted.
        /// </summary>
        float PrivateServiceRoadThreshold { get; }

        /// <summary>
        /// Options for satellite imagery sourcing, including provider URL, tile caching,
        /// and blend settings between real satellite and procedural fake-satellite rendering.
        /// </summary>
        ISatelliteImageOptions Satellite { get; }
    }
}