using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// An <see cref="IElevationGrid"/> implementation that always returns zero elevation.
    /// Used as a stub in tests and demo map generation where real elevation data is not required.
    /// </summary>
    public class FlatElevationGrid : IElevationGrid
    {
        public static readonly FlatElevationGrid Zero = new FlatElevationGrid(0f);

        private readonly float elevation;

        public FlatElevationGrid(float elevation)
        {
            this.elevation = elevation;
        }

        public float ElevationAt(TerrainPoint terrainPoint)
        {
            return elevation;
        }
    }
}
