using GameRealisticMap.Arma3.IO;
using NetTopologySuite.IO.Streams;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal class ShapeFileWriter : IStreamProviderRegistry
    {
        public ShapeFileWriter(IGameFileSystemWriter fileSystemWriter, string path)
        {
            ShapeStream = new GameFileSystemProvider(fileSystemWriter, "SHAPESTREAM", path + ".shp");
            IndexStream = new GameFileSystemProvider(fileSystemWriter, "INDEXSTREAM", path + ".shx");
            DataStream = new GameFileSystemProvider(fileSystemWriter, "DATASTREAM", path + ".dbf");
        }

        public IStreamProvider? this[string streamType] => streamType switch
        {
            "DATASTREAM" => DataStream,
            "SHAPESTREAM" => ShapeStream,
            "INDEXSTREAM" => IndexStream,
            _ => null,
        };

        public GameFileSystemProvider ShapeStream { get; }

        public GameFileSystemProvider IndexStream { get; }

        public GameFileSystemProvider DataStream { get; }
    }
}
