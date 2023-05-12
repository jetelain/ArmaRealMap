using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.IO
{
    public interface IGameFileSystemWriter
    {
        void CreateDirectory(string path);

        void WritePngImage(string path, Image image);

        void WriteTextFile(string path, string text);

        Stream Create(string path);

        bool FileExists(string path);
    }
}
