using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap
{
    public interface ITerrainArea
    {
        TerrainPoint LatLngToTerrainPoint(Coordinate latLng);

        TerrainPolygon ClipArea { get; }
    }
}
