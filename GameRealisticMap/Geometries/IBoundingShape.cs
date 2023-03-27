using GeoAPI.Geometries;

namespace GameRealisticMap.Geometries
{
    public interface IBoundingShape : ITerrainGeometry
    {
        TerrainPoint Center { get; }

        float Angle { get; }

        IPolygon Poly { get; }
    }
}
