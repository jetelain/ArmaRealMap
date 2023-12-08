using GameRealisticMap.Arma3.IO;
using NetTopologySuite.IO.Streams;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    internal sealed class ShapeFileReaderProvider : IStreamProvider
    {
        private IGameFileSystem fileSystem;
        private string path;

        public ShapeFileReaderProvider(IGameFileSystem fileSystem, string kind, string path)
        {
            this.fileSystem = fileSystem;
            this.Kind = kind;
            this.path = path;
        }

        public bool UnderlyingStreamIsReadonly => false;

        public string Kind { get; }

        public Stream OpenRead()
        {
            return fileSystem.OpenFileIfExists(path) ?? throw new FileNotFoundException(null, path);
        }

        public Stream OpenWrite(bool truncate)
        {
            throw new NotImplementedException();
        }
    }
}