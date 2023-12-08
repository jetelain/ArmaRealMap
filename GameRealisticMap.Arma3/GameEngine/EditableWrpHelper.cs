using BIS.WRP;
using GameRealisticMap.ElevationModel;

namespace GameRealisticMap.Arma3.GameEngine
{
    public static class EditableWrpHelper
    {
        public static void FillFromElevationGrid(this EditableWrp wrp, ElevationGrid elevationGrid)
        {
            var size = elevationGrid.Size;
            wrp.Elevation = new float[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    wrp.Elevation[x + (y * size)] = elevationGrid[x, y];
                }
            }
        }

        public static ElevationGrid ToElevationGrid(this EditableWrp wrp)
        {
            var elevationGrid = new ElevationGrid(wrp.TerrainRangeX, wrp.CellSize * wrp.LandRangeX / wrp.TerrainRangeX);
            var size = elevationGrid.Size;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    elevationGrid[x, y] = wrp.Elevation[x + (y * size)];
                }
            }
            return elevationGrid;
        }
    }
}
