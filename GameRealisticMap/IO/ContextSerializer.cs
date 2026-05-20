namespace GameRealisticMap.IO
{
    public class ContextSerializer
    {
        private readonly IBuidersCatalog catalog;

        public ContextSerializer(IBuidersCatalog catalog)
        {
            this.catalog = catalog;
        }

        internal static IDataSerializer<TData> GetSerializer<TData>(IDataBuilder<TData> builder) where TData : class
        {
            return (builder as IDataSerializer<TData>) ?? new DefaultDataSerializer<TData>();
        }

        public IContext ReadLazy(IPackageReader reader)
        {
            return new ContextReader(reader, catalog);
        }

        public async Task<IContext> ReadAll(IPackageReader reader)
        {
            var context = new ContextReader(reader, catalog);
            await context.ReadAll();
            return context;
        }

        public IContext ReadLazyFromDirectory(string path)
        {
            return ReadLazy(new FileSystemPackage(path));
        }

        public Task<IContext> ReadAllFromDirectory(string path)
        {
            return ReadAll(new FileSystemPackage(path));
        }
    }
}
