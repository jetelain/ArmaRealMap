namespace GameRealisticMap.IO
{
    internal class ContextReader : IContext, IDataBuilderVisitor<Task>
    {
        private readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
        private readonly IPackageReader package;
        private readonly IBuidersCatalog catalog;

        public ContextReader(IPackageReader package, IBuidersCatalog catalog)
        {
            this.package = package;
            this.catalog = catalog;
        }

        public T GetData<T>() where T : class
        {
            if (cache.TryGetValue(typeof(T), out var data))
            {
                return (T)data;
            }
            var result = ContextSerializer.GetSerializer(catalog.Get<T>()).Read(package, this).Result;
            if (result == null)
            {
                throw new InvalidOperationException($"Data type '{typeof(T)}' is not supported.");
            }
            cache[typeof(T)] = result;
            return result;
        }

        public async Task Visit<TData>(IDataBuilder<TData> builder) where TData : class
        {
            if (!cache.ContainsKey(typeof(TData)))
            {
                var result = await ContextSerializer.GetSerializer(catalog.Get<TData>()).Read(package, this);
                if (result != null)
                {
                    cache[typeof(TData)] = result;
                }
            }
        }

        public async Task ReadAll()
        {
            foreach (var step in catalog.VisitAll(this))
            {
                await step;
            }
        }
    }
}
