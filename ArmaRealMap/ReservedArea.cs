using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    public class ReservedArea
    {
        public TerrainPolygon Polygon { get; set; }

        public float? Elevation { get; set; }
    }
}