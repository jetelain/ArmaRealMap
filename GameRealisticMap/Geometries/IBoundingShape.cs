using GeoAPI.Geometries;

namespace GameRealisticMap.Geometries
{
    public interface IBoundingShape : ITerrainEnvelope
    {
        TerrainPoint Center { get; }

        float Angle { get; }

        IPolygon Poly { get; }
        TerrainPolygon Polygon { get; }
    }
}
