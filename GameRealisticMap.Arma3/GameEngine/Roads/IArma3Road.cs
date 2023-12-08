using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public interface IArma3Road
    {
        IArma3RoadTypeInfos TypeInfos { get; }

        TerrainPath Path { get; }

        int Order { get; }
    }
}
