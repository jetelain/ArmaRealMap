using System.Numerics;

namespace GameRealisticMap.ElevationModel
{
    public interface IElevationGridConfig
    {
        int Size { get; }

        Vector2 CellSize { get; }
    }
}
