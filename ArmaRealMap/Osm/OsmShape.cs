using NetTopologySuite.Geometries;
using OsmSharp;

namespace ArmaRealMap.Osm
{
    class OsmShape
    {
        internal readonly OsmShapeCategory Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;

        public OsmShape(OsmShapeCategory category, OsmGeo osmGeo, Geometry geometry)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
        }

        public ObjectCategory? BuildingCategory => Category.BuildingType;

    }
}
