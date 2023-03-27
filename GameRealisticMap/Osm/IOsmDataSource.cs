using OsmSharp.Db;
using OsmSharp.Streams;

namespace GameRealisticMap.Osm
{
    public interface IOsmDataSource
    {
        OsmStreamSource Stream { get; }
        SnapshotDb Snapshot { get; }
    }
}
