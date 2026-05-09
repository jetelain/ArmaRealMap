using GeoAPI.Geometries;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test
{
    /// <summary>
    /// Minimal ITerrainArea implementation for unit tests.
    /// Uses an identity coordinate transform (terrain point == lat/lng coords scaled by sizeInMeters).
    /// </summary>
    internal class TerrainAreaMock : ITerrainArea
    {
        private readonly float sizeInMeters;

        public TerrainAreaMock(float sizeInMeters = 1000f, float gridCellSize = 10f)
        {
            this.sizeInMeters = sizeInMeters;
            GridCellSize = gridCellSize;
            GridSize = (int)(sizeInMeters / gridCellSize);
            TerrainBounds = TerrainPolygon.FromRectangle(
                new TerrainPoint(0, 0),
                new TerrainPoint(sizeInMeters, sizeInMeters));
        }

        public float SizeInMeters => sizeInMeters;

        public float GridCellSize { get; }

        public int GridSize { get; }

        public TerrainPolygon TerrainBounds { get; }

        public TerrainPoint LatLngToTerrainPoint(Coordinate latLng)
        {
            // Simple identity-ish mapping for tests: scale lnglat by sizeInMeters
            return new TerrainPoint((float)latLng.X * sizeInMeters, (float)latLng.Y * sizeInMeters);
        }

        public Coordinate TerrainPointToLatLng(TerrainPoint point)
        {
            return new Coordinate(point.X / sizeInMeters, point.Y / sizeInMeters);
        }
    }
}
