namespace GameRealisticMap.Arma3.IO
{
    public interface IGameFileSystem
    {
        IEnumerable<string> FindAll(string pattern);
        Stream? OpenFileIfExists(string path);

        DateTime? GetLastWriteTimeUtc(string path);
    }
}
