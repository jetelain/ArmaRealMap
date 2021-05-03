using NetTopologySuite.Geometries;
using OsmSharp;

namespace ArmaRealMap
{
    class Area
    {
        internal readonly AreaCategory Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;
        private ObjectCategory? buildingCategory;

        public Area(AreaCategory category, OsmGeo osmGeo, Geometry geometry)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
        }

        public ObjectCategory? BuildingCategory
        {
            get { return buildingCategory ?? Category.BuildingType; }
            set { buildingCategory = value; }
        }
    }
}
