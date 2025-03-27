namespace GameRealisticMap.Arma3.IO
{
    public interface IProjectDrive : IGameFileSystem, IGameFileSystemWriter
    {
        string GetFullPath(string path);
    }
}