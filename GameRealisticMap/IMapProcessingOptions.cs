using GameRealisticMap.Configuration;

namespace GameRealisticMap
{
    /// <summary>
    /// Options that control how map data is processed and at what quality or detail level.
    /// Passed to all builders via <see cref="IBuildContext.Options"/>.
    /// 
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
        /// Minimum private service road length in metres to be included in the map. 
        /// Service roads shorter than this threshold are ignored.
        /// 
        /// Applies to roads segments with flag <see cref="ManMade.WaySpecialSegment.PrivateService"/>.
        /// </summary>
        /// <remarks>
        /// This is used for optimization of the map: too many roads can cause performance issues. Default is <c>25</c>.
        /// </remarks>
        float PrivateServiceRoadThreshold { get; }

        /// <summary>
        /// Options for satellite image post processing : contrast, brightness, saturation.
        /// </summary>
        ISatelliteImageOptions Satellite { get; }
    }
}