using System;
using System.Collections.Generic;
using ArmaRealMap.Core.ObjectLibraries;
using GameRealisticMap.Geometries;
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
        private readonly Lazy<IEnumerable<TerrainPath>> terrainPath;

        public OsmShape(OsmShapeCategory category, OsmGeo osmGeo, Geometry geometry, MapInfos mapInfos)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
            this.terrainPolygon = new Lazy<IEnumerable<TerrainPolygon>>(() => TerrainPolygon.FromGeometry(geometry, mapInfos.LatLngToTerrainPoint));
            this.terrainPath = new Lazy<IEnumerable<TerrainPath>>(() => TerrainPath.FromGeometry(geometry, mapInfos.LatLngToTerrainPoint));
        }

        public ObjectCategory? BuildingCategory => Category.BuildingType;

        public IEnumerable<TerrainPolygon> TerrainPolygons => terrainPolygon.Value;

        public IEnumerable<TerrainPath> TerrainPaths => terrainPath.Value;

        public bool IsPath => Geometry.OgcGeometryType == OgcGeometryType.LineString && !((LineString)Geometry).IsClosed;
    }
}
