using System.Text;
using GeoAPI.Geometries;
using Pmad.Cartography;
using OsmSharp;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;

namespace GameRealisticMap.Osm
{
    public sealed class OsmDataSource : IOsmDataSource
    {
        private readonly FeatureInterpreter interpret = new DefaultFeatureInterpreter2();

        private readonly SnapshotDb snapshot;

        public OsmDataSource(ISnapshotDbImpl db)
        {
            snapshot = new SnapshotDb(db);
        }

        internal static OsmDataSource CreateFromXml(string osmFile)
        {
            using (var fileStream = File.OpenRead(osmFile))
            {
                return new OsmDataSource(new MemorySnapshotDb(new XmlOsmStreamSource(fileStream)));
            }
        }

        public static OsmDataSource CreateFromInlineXml(string xml)
        {
            using (var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return new OsmDataSource(new MemorySnapshotDb(new XmlOsmStreamSource(fileStream)));
            }
        }

        internal static OsmDataSource CreateFromPBF(string osmFile)
        {
            using (var fileStream = File.OpenRead(osmFile))
            {
                return new OsmDataSource(new MemorySnapshotDb(new PBFOsmStreamSource(fileStream)));
            }
        }

        public static IOsmDataSource CreateFromFile(string osmFile)
        {
            if (string.Equals(CompressionHelper.GetExtension(osmFile), ".xml", StringComparison.OrdinalIgnoreCase))
            {
                return CompressionHelper.Read(osmFile, stream => new OsmDataSource(new MemorySnapshotDb(new XmlOsmStreamSource(stream))));
            }
            return CompressionHelper.Read(osmFile, stream => new OsmDataSource(new MemorySnapshotDb(new PBFOsmStreamSource(stream))));
        }

        public IEnumerable<OsmGeo> All => snapshot.Get();

        public IEnumerable<Way> Ways => All.OfType<Way>();

        public IEnumerable<Node> Nodes => All.OfType<Node>();

        public IEnumerable<Relation> Relations => All.OfType<Relation>();

        public SnapshotDb SnapshotDb => snapshot;

        public IEnumerable<IGeometry> Interpret(OsmGeo osmGeo)
        {
            return interpret.Interpret(osmGeo, snapshot).Features.Select(f => f.Geometry);
        }
    }
}
