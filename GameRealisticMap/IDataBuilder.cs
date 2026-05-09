using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    /// <summary>
    /// Builds a single data type <typeparamref name="T"/> from an <see cref="IBuildContext"/>.
    /// Register implementations in <see cref="IBuidersCatalog"/> so that
    /// <see cref="IContext.GetData{T}"/> can resolve and cache them automatically.
    /// </summary>
    /// <typeparam name="T">The type of data produced by this builder. Must be a reference type.</typeparam>
    public interface IDataBuilder<out T> where T : class
    {
        /// <summary>
        /// Builds and returns the data object.
        /// Called at most once per <see cref="BuildContext"/>; results are cached automatically.
        /// Request dependencies via <paramref name="context"/>.<see cref="IContext.GetData{T}"/>.
        /// </summary>
        /// <param name="context">The build context providing OSM data, terrain area, and access to other built data.</param>
        /// <param name="scope">Progress scope for reporting sub-operation progress.</param>
        /// <returns>The fully built data object.</returns>
        T Build(IBuildContext context, IProgressScope scope);
    }
}
