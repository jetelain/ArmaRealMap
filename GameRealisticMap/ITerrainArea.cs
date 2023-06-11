using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap
{
    public interface ITerrainArea
    {
        TerrainPoint LatLngToTerrainPoint(Coordinate latLng);

        Coordinate TerrainPointToLatLng(TerrainPoint point);

        TerrainPolygon TerrainBounds { get; }

        float GridCellSize { get; }

        int GridSize { get; }

        float SizeInMeters { get; }
    }
}
