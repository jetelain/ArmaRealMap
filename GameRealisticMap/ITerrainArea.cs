using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap
{
    public interface ITerrainArea
    {
        TerrainPoint LatLngToTerrainPoint(Coordinate latLng);

        Coordinate TerrainPointToLatLng(TerrainPoint point);

        IEnumerable<Coordinate> GetLatLngBounds();

        TerrainPolygon TerrainBounds { get; }

        float GridCellSize { get; }

        int GridSize { get; }
    }
}
