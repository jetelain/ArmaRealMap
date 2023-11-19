using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    internal sealed class Arma3RoadWrapper : IArma3Road
    {
        public Arma3RoadWrapper(Road road)
        {
            TypeInfos = (IArma3RoadTypeInfos)road.RoadTypeInfos;

            if (road.RoadType < RoadTypeId.SingleLaneDirtPath)
            {
                Path = road.Path.PreventSplines(road.Width * 1.5f);
            }
            else
            {
                Path = road.Path;
            }
        }

        public IArma3RoadTypeInfos TypeInfos { get; }

        public TerrainPath Path { get; }

        public int Order => TypeInfos.Id;
    }
}
