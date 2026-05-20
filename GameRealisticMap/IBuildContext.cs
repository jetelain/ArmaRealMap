using GameRealisticMap.Osm;

namespace GameRealisticMap
{
    /// <summary>
    /// The central context object passed to all <see cref="IDataBuilder{T}"/> instances during map generation.
    /// Extends <see cref="IContext"/> with geographic area information, OSM data, and processing options.
    /// </summary>
    public interface IBuildContext : IContext
    {
        /// <summary>
        /// The OpenStreetMap data source containing all OSM features (nodes, ways, relations)
        /// clipped to the terrain area. Used by all OSM-based builders.
        /// </summary>
        IOsmDataSource OsmSource { get; }

        /// <summary>
        /// Processing options that control imagery resolution, road classification thresholds,
        /// and satellite image post processing settings.
        /// </summary>
        IMapProcessingOptions Options { get; }
    }
}