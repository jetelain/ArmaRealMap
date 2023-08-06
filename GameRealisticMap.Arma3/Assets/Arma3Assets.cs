using System.Text.Json;
using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    public class Arma3Assets : Arma3AssetsDependenciesOnly, IArma3RegionAssets, IRoadTypeLibrary<Arma3RoadTypeInfos>
    {
        public const string BuiltinPrefix = "builtin:";
        private const string BuiltinNamespace = "GameRealisticMap.Arma3.Builtin.";

        public Dictionary<BasicCollectionId, List<BasicCollectionDefinition>> BasicCollections { get; set; } = new ();
        
        public Dictionary<RoadTypeId, BridgeDefinition> Bridges { get; set; } = new ();
        
        public Dictionary<BuildingTypeId, List<BuildingDefinition>> Buildings { get; set; } = new ();
        
        public Dictionary<ClusterCollectionId, List<ClusterCollectionDefinition>> ClusterCollections { get; set; } = new ();

        public TerrainMaterialLibrary Materials { get; set; } = new ();

        public List<Arma3RoadTypeInfos> Roads { get; set; } = new ();

        public Dictionary<ObjectTypeId, List<ObjectDefinition>> Objects { get; set; } = new ();

        public Dictionary<PondSizeId, ModelInfo> Ponds { get; set; } = new ();

        public Dictionary<FenceTypeId, List<FenceDefinition>> Fences { get; set; } = new();

        public RailwaysDefinition? Railways { get; set; }

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

        public IReadOnlyCollection<BuildingDefinition> GetBuildings(BuildingTypeId buildingTypeId)
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

        public ModelInfo? GetPond(PondSizeId pondSize)
        {
            if (Ponds.TryGetValue(pondSize, out var value))
            {
                return value;
            }
            return null;
        }

        IRoadTypeLibrary<Arma3RoadTypeInfos> IArma3RegionAssets.RoadTypeLibrary => this;

        public string BaseWorldName { get; set; } = "arm_world_centraleurope";

        public string BaseDependency { get; set; } = "arm_centraleurope";
        
        public static JsonSerializerOptions CreateJsonSerializerOptions(IModelInfoLibrary library, bool allowUnresolvedModel = false)
        {
            return new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new ModelInfoReferenceConverter(library, allowUnresolvedModel)
                },
                WriteIndented = true
            };
        }

        public IReadOnlyCollection<FenceDefinition> GetFences(FenceTypeId typeId)
        {
            return Lookup(Fences, typeId);
        }

        public static List<string> GetBuiltinList()
        {
            return typeof(Arma3Assets).Assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(BuiltinNamespace, StringComparison.Ordinal))
                .Select(r => BuiltinPrefix + r.Substring(BuiltinNamespace.Length))
                .OrderBy(r => r)
                .ToList();
        }

        public static async Task<Arma3Assets> LoadFromFile(ModelInfoLibrary library, string path, bool allowUnresolvedModel = false)
        {
            Arma3Assets? assets = null;

            if (path.StartsWith(BuiltinPrefix))
            {
                var filename = path.Substring(BuiltinPrefix.Length);
                using (var source = typeof(Arma3Assets).Assembly.GetManifestResourceStream(BuiltinNamespace + filename))
                {
                    if (source == null)
                    {
                        throw new FileNotFoundException($"Builtin file '{filename}' does not exists (name is case sensitive).");
                    }
                    assets = await JsonSerializer.DeserializeAsync<Arma3Assets>(source, CreateJsonSerializerOptions(library, allowUnresolvedModel));
                }
            }
            else
            {
                using (var source = File.OpenRead(path))
                {
                    assets = await JsonSerializer.DeserializeAsync<Arma3Assets>(source, CreateJsonSerializerOptions(library, allowUnresolvedModel));
                }
            }
            if ( assets == null)
            {
                throw new IOException("Invalid JSON file");
            }
            return assets;
        }

        public static async Task<Arma3AssetsDependenciesOnly> LoadDependenciesFromFile(string path)
        {
            Arma3AssetsDependenciesOnly? assets = null;
            if (path.StartsWith(BuiltinPrefix))
            {
                var filename = path.Substring(BuiltinPrefix.Length);
                using (var source = typeof(Arma3Assets).Assembly.GetManifestResourceStream(BuiltinNamespace + filename))
                {
                    if (source == null)
                    {
                        throw new FileNotFoundException($"Builtin file '{filename}' does not exists (name is case sensitive).");
                    }
                    assets = await JsonSerializer.DeserializeAsync<Arma3AssetsDependenciesOnly>(source);
                }
            }
            else
            {
                using (var source = File.OpenRead(path))
                {
                    assets = await JsonSerializer.DeserializeAsync<Arma3AssetsDependenciesOnly>(source);
                }
            }
            if (assets == null)
            {
                throw new IOException("Invalid JSON file");
            }
            return assets;
        }
    }
}
