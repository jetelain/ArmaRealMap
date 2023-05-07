using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    internal interface IArma3RegionAssets
    {
        IRoadTypeLibrary<IArma3RoadTypeInfos> RoadTypeLibrary { get; }

        ITerrainMaterialLibrary Materials { get; }

        List<BuildingDefinition> Buildings { get; }

        ModelInfo GetPond(int pondSize);
    }
}
