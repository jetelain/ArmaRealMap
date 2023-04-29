using System.IO.Compression;

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

        public async Task Write(IPackageWriter writer, IContext context)
        {
            foreach(var step in catalog.VisitAll(new ContextWriter(writer, context)))
            {
                await step;
            }
        }

        public Task WriteToDirectory(string path, IContext context)
        {
            return Write(new FileSystemPackage(path), context);
        }

        public async Task WriteToZip(string path, IContext context)
        {
            using var archive = new ZipArchive(File.Create(path), ZipArchiveMode.Create);

            await Write(new ZipPackageWriter(archive), context);
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
