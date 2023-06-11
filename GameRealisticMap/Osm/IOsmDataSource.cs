using GeoAPI.Geometries;
using OsmSharp;
using OsmSharp.Db;

namespace GameRealisticMap.Osm
{
    public interface IOsmDataSource
    {
        IEnumerable<OsmGeo> All { get; }

        IEnumerable<Way> Ways { get; }

        IEnumerable<Node> Nodes { get; }

        IEnumerable<Relation> Relations { get; }

        IEnumerable<IGeometry> Interpret(OsmGeo osmGeo);

        SnapshotDb SnapshotDb { get; }
    }
}
