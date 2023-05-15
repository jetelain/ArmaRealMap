using GameRealisticMap.Arma3.IO;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    internal class GameFileSystemMock : IGameFileSystemWriter
    {
        public Stream Create(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public Stream? OpenFileIfExists(string path)
        {
            throw new NotImplementedException();
        }

        public void WritePngImage(string path, Image image)
        {
            throw new NotImplementedException();
        }

        public void WriteTextFile(string path, string text)
        {
            throw new NotImplementedException();
        }
    }
}