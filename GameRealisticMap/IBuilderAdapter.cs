namespace GameRealisticMap
{
    /// <summary>
    /// Internal type-erased wrapper for a typed <see cref="IDataBuilder{T}"/> registration.
    /// Allows <see cref="BuildersCatalog"/> to store and invoke builders without knowing
    /// their concrete output type at compile time.
    /// </summary>
    internal interface IBuilderAdapter
    {
        /// <summary>
        /// The underlying builder, type-erased to <c>IDataBuilder&lt;object&gt;</c>.
        /// </summary>
        IDataBuilder<object> Builder { get; }

        /// <summary>
        /// Dispatches to the visitor using the builder's concrete output type,
        /// enabling type-safe visitor operations without reflection at the call site.
        /// </summary>
        TResult Accept<TResult>(IDataBuilderVisitor<TResult> visitor);

        /// <summary>
        /// Synchronously retrieves or builds the data object from the given context.
        /// </summary>
        object Get(IContext ctx);

        /// <summary>
        /// Asynchronously retrieves or builds the data object from the given context.
        /// </summary>
        Task<object> GetAsync(IContext ctx);
    }
}