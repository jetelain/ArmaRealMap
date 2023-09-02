using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    public interface IArma3RegionAssets
    {
        IRoadTypeLibrary<Arma3RoadTypeInfos> RoadTypeLibrary { get; }

        TerrainMaterialLibrary Materials { get; }

        string BaseWorldName { get; }

        string BaseDependency { get; }

        IReadOnlyCollection<BuildingDefinition> GetBuildings(BuildingTypeId buildingTypeId);
        
        IReadOnlyCollection<ObjectDefinition> GetObjects(ObjectTypeId typeId);

        IReadOnlyCollection<BasicCollectionDefinition> GetBasicCollections(BasicCollectionId basicId);

        IReadOnlyCollection<ClusterCollectionDefinition> GetClusterCollections(ClusterCollectionId clustersId);

        ModelInfo? GetPond(PondSizeId pondSize);

        BridgeDefinition? GetBridge(RoadTypeId roadType);

        IReadOnlyCollection<FenceDefinition> GetFences(FenceTypeId typeId);

        IReadOnlyCollection<RowDefinition> GetNaturalRows(NaturalRowType typeId);

        RailwaysDefinition? Railways { get; }

        IReadOnlyCollection<ModDependencyDefinition> Dependencies { get; }

        IReadOnlyCollection<SidewalksDefinition> Sidewalks { get; }
    }
}
