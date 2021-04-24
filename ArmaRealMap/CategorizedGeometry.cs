using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using OsmSharp;

namespace ArmaRealMap
{
    class CategorizedGeometry
    {
        internal readonly Category Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;

        public CategorizedGeometry(Category category, OsmGeo osmGeo, Geometry geometry)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
        }
    }
}
