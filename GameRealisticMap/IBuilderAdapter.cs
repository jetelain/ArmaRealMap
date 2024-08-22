namespace GameRealisticMap
{
    internal interface IBuilderAdapter
    {
        IDataBuilder<object> Builder { get; }

        TResult Accept<TResult>(IDataBuilderVisitor<TResult> visitor);

        object Get(IContext ctx);

        Task<object> GetAsync(IContext ctx);
    }
}