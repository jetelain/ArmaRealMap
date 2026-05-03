namespace GameRealisticMap
{
    /// <summary>
    /// Registry that maps data types to their <see cref="IDataBuilder{T}"/> implementations.
    /// Used by <see cref="BuildContext"/> to resolve builders when data is requested via
    /// <see cref="IContext.GetData{T}"/>. The default implementation is <see cref="BuildersCatalog"/>.
    /// </summary>
    /// <remarks>
    /// The name "IBuidersCatalog" (missing 'd' in "Builders") is a historical typo that is
    /// intentionally preserved for backwards compatibility — do not rename.
    /// </remarks>
    public interface IBuidersCatalog
    {
        /// <summary>
        /// Registers a builder for data type <typeparamref name="TData"/>.
        /// Each data type may only have one builder registered; registering a second builder
        /// for the same type will overwrite the first.
        /// </summary>
        /// <typeparam name="TData">The data type produced by <paramref name="builder"/>.</typeparam>
        /// <param name="builder">The builder instance to register.</param>
        void Register<TData>(IDataBuilder<TData> builder)
            where TData : class;

        /// <summary>
        /// Retrieves the registered builder for data type <typeparamref name="TData"/>.
        /// </summary>
        /// <typeparam name="TData">The data type whose builder to retrieve.</typeparam>
        /// <returns>The registered builder.</returns>
        /// <exception cref="NotSupportedException">Thrown if no builder is registered for <typeparamref name="TData"/>.</exception>
        IDataBuilder<TData> Get<TData>() 
            where TData : class;

        /// <summary>
        /// Visits all registered builders using the provided visitor, returning one result per builder.
        /// Useful for generating reports, documentation, or performing bulk operations on all builders.
        /// </summary>
        /// <typeparam name="TResult">The result type returned by the visitor for each builder.</typeparam>
        /// <param name="visitor">The visitor to apply to each registered builder.</param>
        IEnumerable<TResult> VisitAll<TResult>(IDataBuilderVisitor<TResult> visitor);

        /// <summary>
        /// Builds (if not already cached) and returns all data objects in this context whose
        /// concrete type is assignable to <typeparamref name="T"/>.
        /// An optional <paramref name="filter"/> predicate can restrict which registered types are included.
        /// </summary>
        /// <typeparam name="T">The base type to filter by.</typeparam>
        /// <param name="ctx">The context used to retrieve (and build if needed) each data object.</param>
        /// <param name="filter">Optional predicate on the concrete data type. Pass <c>null</c> to include all matching types.</param>
        IEnumerable<T> GetOfType<T>(IContext ctx, Func<Type, bool>? filter = null) where T : class;

        /// <summary>
        /// Builds (if not already cached) and returns all data objects for all registered builders.
        /// </summary>
        /// <param name="ctx">The context used to retrieve each data object.</param>
        IEnumerable<object> GetAll(IContext ctx);
    }
}