namespace GameRealisticMap
{
    public interface IBuidersCatalog
    {
        void Register<TData>(IDataBuilder<TData> builder)
            where TData : class;

        IDataBuilder<TData> Get<TData>() 
            where TData : class;

        IEnumerable<TResult> VisitAll<TResult>(IDataBuilderVisitor<TResult> visitor);

        IEnumerable<T> GetOfType<T>(IContext ctx, Func<Type, bool>? filter = null) where T : class;

        Task<IEnumerable<T>> GetOfTypeAsync<T>(IContext ctx, Func<Type, bool>? filter = null) where T : class;
    }
}