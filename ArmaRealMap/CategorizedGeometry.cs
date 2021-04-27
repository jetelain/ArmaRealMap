using NetTopologySuite.Geometries;
using OsmSharp;

namespace ArmaRealMap
{
    class CategorizedGeometry
    {
        internal readonly Category Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;
        private BuildingCategory? buildingCategory;

        public CategorizedGeometry(Category category, OsmGeo osmGeo, Geometry geometry)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
        }

        public BuildingCategory? BuildingCategory
        {
            get { return buildingCategory ?? Category.BuildingType; }
            set { buildingCategory = value; }
        }
    }
}
