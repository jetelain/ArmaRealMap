namespace GameRealisticMap
{
    /// <summary>
    /// Visitor interface for iterating over all builders registered in an <see cref="IBuidersCatalog"/>.
    /// Used with <see cref="IBuidersCatalog.VisitAll{TResult}"/> to perform type-safe operations
    /// across all registered builders without exposing catalog internals.
    /// </summary>
    /// <typeparam name="TResult">The result type returned for each visited builder.</typeparam>
    public interface IDataBuilderVisitor<TResult>
    {
        /// <summary>
        /// Called once for each builder registered in the catalog.
        /// Implement to inspect, transform, or collect information from the builder.
        /// </summary>
        /// <typeparam name="TData">The concrete data type produced by <paramref name="builder"/>.</typeparam>
        /// <param name="builder">The builder being visited.</param>
        /// <returns>A result value for this builder, collected into the sequence returned by <see cref="IBuidersCatalog.VisitAll{TResult}"/>.</returns>
        TResult Visit<TData>(IDataBuilder<TData> builder) where TData : class;
    }
}