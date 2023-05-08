using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    internal interface IArma3RegionAssets
    {
        IRoadTypeLibrary<IArma3RoadTypeInfos> RoadTypeLibrary { get; }

        ITerrainMaterialLibrary Materials { get; }

        IEnumerable<BuildingDefinition> GetBuildings(BuildingTypeId buildingTypeId);

        IReadOnlyList<ObjectDefinition> GetObjects(ObjectTypeId typeId);

        IReadOnlyCollection<IBasicDefinition<Composition>> GetBasic(BasicId basicId);

        IReadOnlyCollection<ClusterCollectionDefinition> GetClusterCollection(ClusterCollectionId clustersId);

        ModelInfo GetPond(int pondSize);
    }
}
