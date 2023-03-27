using NetTopologySuite.Geometries;

namespace GameRealisticMap.Geometries
{
    public interface IBoundingShape : ITerrainGeometry
    {
        TerrainPoint Center { get; }

        float Angle { get; }

        Polygon Poly { get; }
    }
}
