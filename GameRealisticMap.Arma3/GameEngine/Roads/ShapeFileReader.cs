using GameRealisticMap.Arma3.IO;
using NetTopologySuite.IO.Streams;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    internal sealed class ShapeFileReader : IStreamProviderRegistry
    {
        public ShapeFileReader(IGameFileSystem fileSystem, string path)
        {
            ShapeStream = new ShapeFileReaderProvider(fileSystem, "SHAPESTREAM", path + ".shp");
            IndexStream = new ShapeFileReaderProvider(fileSystem, "INDEXSTREAM", path + ".shx");
            DataStream = new ShapeFileReaderProvider(fileSystem, "DATASTREAM", path + ".dbf");
        }

        public IStreamProvider? this[string streamType] => streamType switch
        {
            "DATASTREAM" => DataStream,
            "SHAPESTREAM" => ShapeStream,
            "INDEXSTREAM" => IndexStream,
            _ => null,
        };

        public ShapeFileReaderProvider ShapeStream { get; }

        public ShapeFileReaderProvider IndexStream { get; }

        public ShapeFileReaderProvider DataStream { get; }
    }
}
