using System.Numerics;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Read-only configuration properties of an elevation grid:
    /// cell size and grid dimensions.
    /// </summary>
    public interface IElevationGridConfig
    {
        int Size { get; }

        Vector2 CellSize { get; }
    }
}
