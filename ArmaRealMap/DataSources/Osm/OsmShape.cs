using System;
using System.Collections.Generic;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Geometries;
using NetTopologySuite.Geometries;
using OsmSharp;

namespace ArmaRealMap.Osm
{
    class OsmShape
    {
        internal readonly OsmShapeCategory Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;
        private readonly Lazy<IEnumerable<TerrainPolygon>> terrainPolygon;

        public OsmShape(OsmShapeCategory category, OsmGeo osmGeo, Geometry geometry, MapInfos mapInfos)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
            this.terrainPolygon = new Lazy<IEnumerable<TerrainPolygon>>(() => TerrainPolygon.FromGeometry(geometry, mapInfos.LatLngToTerrainPoint));
        }

        public ObjectCategory? BuildingCategory => Category.BuildingType;

        public IEnumerable<TerrainPolygon> TerrainPolygons => terrainPolygon.Value;
    }
}
