namespace GameRealisticMap
{
    internal class BuilderAdapter<TData> : IBuilderAdapter
        where TData : class
    {
        private readonly IDataBuilder<TData> builder;

        public BuilderAdapter(IDataBuilder<TData> builder)
        {
            this.builder = builder;
        }

        public Type DataType => typeof(TData);

        public IDataBuilder<object> Builder => builder;

        public object Get(IContext ctx)
        {
            return ctx.GetData<TData>();
        }

        public async Task<object> GetAsync(IContext ctx)
        {
            return await ctx.GetDataAsync<TData>();
        }

        public TResult Accept<TResult>(IDataBuilderVisitor<TResult> visitor)
        {
            return visitor.Visit<TData>(builder);
        }
    }
}
