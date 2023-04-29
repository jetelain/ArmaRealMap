namespace GameRealisticMap.IO
{
    internal class DefaultDataSerializer<T> : IDataSerializer<T> where T : class
    {
        private readonly string filename = typeof(T).Name.Replace("Data", "") + ".json";

        public async ValueTask<T?> Read(IPackageReader package, IContext context)
        {
            return await package.ReadJson<T>(filename);
        }

        public Task Write(IPackageWriter package, T data)
        {
            return package.WriteJson<T>(filename, data);
        }
    }
}
