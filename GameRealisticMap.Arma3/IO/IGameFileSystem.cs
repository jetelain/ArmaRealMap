namespace GameRealisticMap.Arma3.IO
{
    public interface IGameFileSystem
    {
        Stream? OpenFileIfExists(string path);
    }
}
