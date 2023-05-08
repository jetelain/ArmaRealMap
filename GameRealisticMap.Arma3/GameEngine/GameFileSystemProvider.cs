using GameRealisticMap.Arma3.IO;
using NetTopologySuite.IO.Streams;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal class GameFileSystemProvider : IStreamProvider
    {
        private IGameFileSystemWriter fileSystemWriter;
        private string path;

        public GameFileSystemProvider(IGameFileSystemWriter fileSystemWriter, string kind, string path)
        {
            this.fileSystemWriter = fileSystemWriter;
            this.Kind = kind;
            this.path = path;
        }

        public bool UnderlyingStreamIsReadonly => false;

        public string Kind { get; }

        public Stream OpenRead()
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite(bool truncate)
        {
            // always truncate to be sure to always start from a clean file
            return fileSystemWriter.Create(path);
        }
    }
}