using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    /// <summary>
    /// Asynchronous variant of <see cref="IDataBuilder{T}"/> for builders that perform I/O
    /// (e.g. downloading elevation tiles from NASA SRTM or satellite imagery from Sentinel-2).
    /// The synchronous <see cref="IDataBuilder{T}.Build"/> method is provided as a default
    /// interface implementation that blocks on <see cref="BuildAsync"/>.
    /// </summary>
    /// <typeparam name="T">The type of data produced by this builder. Must be a reference type.</typeparam>
    internal interface IDataBuilderAsync<T> : IDataBuilder<T> where T : class
    {
        T IDataBuilder<T>.Build(IBuildContext context, IProgressScope scope)
        {
            return BuildAsync(context, scope).Result;
        }

        /// <summary>
        /// Asynchronously builds and returns the data object.
        /// Called at most once per <see cref="BuildContext"/>; results are cached automatically.
        /// </summary>
        /// <param name="context">The build context providing access to dependencies and OSM data.</param>
        /// <param name="scope">Progress scope for reporting download and processing progress.</param>
        /// <returns>A task representing the asynchronous build operation.</returns>
        Task<T> BuildAsync(IBuildContext context, IProgressScope scope);
    }
}