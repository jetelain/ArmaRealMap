namespace GameRealisticMap
{
    public interface IDataBuilderVisitor<TResult>
    {
        TResult Visit<TData>(IDataBuilder<TData> builder) where TData : class;
    }
}