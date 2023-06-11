using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
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
