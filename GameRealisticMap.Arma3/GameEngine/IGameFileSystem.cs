using System.Diagnostics.CodeAnalysis;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IGameFileSystem
    {
        Stream? TryOpenFile(string path);
    }
}
