using GeoAPI.Geometries;
using OsmSharp;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;

namespace GameRealisticMap.Osm
{
    internal class OsmDataSource : IOsmDataSource
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

        internal static OsmDataSource CreateFromPBF(string osmFile)
        {
            using (var fileStream = File.OpenRead(osmFile))
            {
                return new OsmDataSource(new MemorySnapshotDb(new PBFOsmStreamSource(fileStream)));
            }
        }


        public IEnumerable<OsmGeo> All => snapshot.Get();
        public IEnumerable<Way> Ways => All.OfType<Way>();

        public IEnumerable<IGeometry> Interpret(OsmGeo osmGeo)
        {
            return interpret.Interpret(osmGeo, snapshot).Features.Select(f => f.Geometry);
        }
    }
}
