namespace GameRealisticMap.IO
{
    public interface IDataSerializer<T> where T : class
    {
        ValueTask<T?> Read(IPackageReader package, IContext context);

        Task Write(IPackageWriter package, T data);
    }
}
