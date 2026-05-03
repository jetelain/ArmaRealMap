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
        /// Returns all built data objects whose type is assignable to <typeparamref name="T"/>.
        /// Only includes data types that have already been built and cached in this context.
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
