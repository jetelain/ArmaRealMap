using Pmad.HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    /// <summary>
    /// Base context for accessing built data in a map generation pipeline.
    /// Data is built lazily on first request and cached for subsequent calls.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// The geographic area being processed. Provides coordinate conversion between
        /// WGS-84 lat/lng and local terrain space (<see cref="Geometries.TerrainPoint"/>),
        /// and exposes the terrain grid dimensions.
        /// </summary>
        ITerrainArea Area { get; }

        /// <summary>
        /// Gets or builds data of type <typeparamref name="T"/> synchronously.
        /// If the data has already been built, returns the cached result.
        /// Blocks the calling thread until the data is available.
        /// </summary>
        /// <typeparam name="T">The data type to retrieve. Must have a registered <see cref="IDataBuilder{T}"/>.</typeparam>
        /// <param name="parentScope">Optional progress scope to nest the build operation under.</param>
        T GetData<T>(IProgressScope? parentScope = null) where T : class;

        /// <summary>
        /// Gets or builds data of type <typeparamref name="T"/> asynchronously.
        /// If the data has already been built, returns the cached result.
        /// </summary>
        /// <typeparam name="T">The data type to retrieve. Must have a registered <see cref="IDataBuilder{T}"/>.</typeparam>
        /// <param name="parentScope">Optional progress scope to nest the build operation under.</param>
        Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class;

        /// <summary>
        /// Builds (if not already cached) and returns all data objects in this context whose
        /// concrete type is assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The base type to filter by.</typeparam>
        IEnumerable<T> GetOfType<T>() where T : class;

        /// <summary>
        /// Storage for large raster images (satellite tiles, elevation grids) that exceed
        /// practical in-memory limits. Images are memory-mapped to disk as needed.
        /// </summary>
        IHugeImageStorage HugeImageStorage { get; }
    }
}
