using System.Text.Json;
using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    public class Arma3Assets : IArma3RegionAssets, IRoadTypeLibrary<Arma3RoadTypeInfos>
    {
        public Dictionary<BasicCollectionId, List<BasicCollectionDefinition>> BasicCollections { get; set; } = new Dictionary<BasicCollectionId, List<BasicCollectionDefinition>>();
        
        public Dictionary<RoadTypeId, BridgeDefinition> Bridges { get; set; } = new Dictionary<RoadTypeId, BridgeDefinition>();
        
        public Dictionary<BuildingTypeId, List<BuildingDefinition>> Buildings { get; set; } = new Dictionary<BuildingTypeId, List<BuildingDefinition>>();
        
        public Dictionary<ClusterCollectionId, List<ClusterCollectionDefinition>> ClusterCollections { get; set; } = new Dictionary<ClusterCollectionId, List<ClusterCollectionDefinition>>();

        public TerrainMaterialLibrary Materials { get; set; } = new TerrainMaterialLibrary();

        public List<Arma3RoadTypeInfos> Roads { get; set; } = new List<Arma3RoadTypeInfos>();

        public Dictionary<ObjectTypeId, List<ObjectDefinition>> Objects { get; set; } = new Dictionary<ObjectTypeId, List<ObjectDefinition>>();

        public Dictionary<PondSizeId, ModelInfo> Ponds { get; set; } = new Dictionary<PondSizeId, ModelInfo>();

        public IReadOnlyCollection<BasicCollectionDefinition> GetBasicCollections(BasicCollectionId basicId)
        {
            return Lookup(BasicCollections, basicId);
        }

        public BridgeDefinition? GetBridge(RoadTypeId roadType)
        {
            if (Bridges.TryGetValue(roadType, out var bridge))
            {
                return bridge;
            }
            return null;
        }

        public IEnumerable<BuildingDefinition> GetBuildings(BuildingTypeId buildingTypeId)
        {
            if (!Buildings.TryGetValue(buildingTypeId, out var buildings))
            {
                buildings = new List<BuildingDefinition>();
            }
            return buildings;
        }

        public IReadOnlyCollection<ClusterCollectionDefinition> GetClusterCollections(ClusterCollectionId clustersId)
        {
            return Lookup(ClusterCollections, clustersId);
        }

        public Arma3RoadTypeInfos GetInfo(RoadTypeId id)
        {
            return Roads.First(m => m.Id == id);
        }

        public IReadOnlyCollection<ObjectDefinition> GetObjects(ObjectTypeId typeId)
        {
            return Lookup(Objects, typeId);
        }

        private IReadOnlyCollection<T> Lookup<K,T>(Dictionary<K, List<T>> dict, K typeId)
            where K : notnull
            where T : IWithProbability
        {
            if (!dict.TryGetValue(typeId, out var objects))
            {
                objects = new List<T>();
            }
            objects.CheckProbabilitySum();
            return objects;
        }

        public ModelInfo GetPond(PondSizeId pondSize)
        {
            return Ponds[pondSize];
        }

        IRoadTypeLibrary<Arma3RoadTypeInfos> IArma3RegionAssets.RoadTypeLibrary => this;

        public static JsonSerializerOptions CreateJsonSerializerOptions(IModelInfoLibrary library)
        {
            return new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new ModelInfoReferenceConverter(library)
                },
                WriteIndented = true
            };
        }
    }
}
