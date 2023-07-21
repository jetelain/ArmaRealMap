using GeoAPI.Geometries;
using OsmSharp;
using OsmSharp.Db;
using OsmSharp.Db.Impl;

namespace GameRealisticMap.Osm
{
    public sealed class NoneOsmDataSource : IOsmDataSource
    {
        public IEnumerable<OsmGeo> All => Enumerable.Empty<OsmGeo>();

        public IEnumerable<Way> Ways => Enumerable.Empty<Way>();

        public IEnumerable<Node> Nodes => Enumerable.Empty<Node>();

        public IEnumerable<Relation> Relations => Enumerable.Empty<Relation>();

        public SnapshotDb SnapshotDb { get; } = new SnapshotDb(new MemorySnapshotDb());

        public IEnumerable<IGeometry> Interpret(OsmGeo osmGeo)
        {
            return Enumerable.Empty<IGeometry>();
        }
    }
}
