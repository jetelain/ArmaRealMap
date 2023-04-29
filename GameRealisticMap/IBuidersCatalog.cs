namespace GameRealisticMap
{
    public interface IBuidersCatalog
    {
        void Register<TData>(IDataBuilder<TData> builder)
            where TData : class;

        IDataBuilder<TData> Get<TData>() 
            where TData : class;

        IEnumerable<TResult> VisitAll<TResult>(IDataBuilderVisitor<TResult> visitor);
        //IEnumerable<TResult> Visit<TResult>(IDataBuilderVisitor<TResult> visitor);
    }
}