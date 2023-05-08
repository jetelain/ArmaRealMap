using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    internal interface IArma3RegionAssets
    {
        IRoadTypeLibrary<Arma3RoadTypeInfos> RoadTypeLibrary { get; }

        ITerrainMaterialLibrary Materials { get; }

        IEnumerable<BuildingDefinition> GetBuildings(BuildingTypeId buildingTypeId);

        IReadOnlyCollection<ObjectDefinition> GetObjects(ObjectTypeId typeId);

        IReadOnlyCollection<BasicCollectionDefinition> GetBasicCollections(BasicCollectionId basicId);

        IReadOnlyCollection<ClusterCollectionDefinition> GetClusterCollections(ClusterCollectionId clustersId);

        ModelInfo GetPond(int pondSize);

        BridgeDefinition? GetBridge(RoadTypeId roadType);
    }
}
