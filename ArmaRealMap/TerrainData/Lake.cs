using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    internal class Lake : ITerrainGeometry
    {
        public float BorderElevation { get; internal set; }
        public float WaterElevation { get; internal set; }
        public TerrainPolygon TerrainPolygon { get; internal set; }

        public TerrainPoint MinPoint => TerrainPolygon.MinPoint;
        public TerrainPoint MaxPoint => TerrainPolygon.MaxPoint;
    }
}