using GeoAPI.Geometries;
using OsmSharp;

namespace GameRealisticMap.Osm
{
    public interface IOsmDataSource
    {
        IEnumerable<OsmGeo> All { get; }

        IEnumerable<Way> Ways { get; }

        IEnumerable<Node> Nodes { get; }

        IEnumerable<IGeometry> Interpret(OsmGeo osmGeo);
    }
}
