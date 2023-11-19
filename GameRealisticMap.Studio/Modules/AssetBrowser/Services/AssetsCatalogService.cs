using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BIS.P3D;
using GameRealisticMap.Studio.Modules.Arma3Data;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    [Export(typeof(IAssetsCatalogService))]
    internal class AssetsCatalogService : IAssetsCatalogService
    {
        private static readonly Logger logger = LogManager.GetLogger("AssetsCatalogService");

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private List<AssetCatalogItem>? cachedList;

        private readonly IArma3DataModule arma3DataModule;
        public string AssetsCatalogPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "assets.json");

        [ImportingConstructor]
        public AssetsCatalogService(IArma3DataModule arma3DataModule)
        {
            this.arma3DataModule = arma3DataModule;
        }

        public async Task<List<AssetCatalogItem>> GetOrLoad()
        {
            if (cachedList == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (cachedList == null)
                    {
                        cachedList = await EnsureLibraryModels(await LoadFromFile().ConfigureAwait(false)).ConfigureAwait(false);
                    }
                }
                finally
                { 
                    semaphoreSlim.Release(); 
                }
            }
            return cachedList;
        }

        private async Task<List<AssetCatalogItem>> LoadFromFile()
        {
            if (File.Exists(AssetsCatalogPath))
            {
                using var stream = File.OpenRead(AssetsCatalogPath);
                return await JsonSerializer.DeserializeAsync<List<AssetCatalogItem>>(stream, new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter() } }).ConfigureAwait(false)
                    ?? new List<AssetCatalogItem>();
            }
            return new List<AssetCatalogItem>();
        }

        private async Task<List<AssetCatalogItem>> EnsureLibraryModels(List<AssetCatalogItem> items)
        {
            await EnsurePaths(items, arma3DataModule.Library.Models.Select(m => m.Path)).ConfigureAwait(false);
            return items;
        }

        private async Task EnsurePaths(List<AssetCatalogItem> items, IEnumerable<string> paths)
        {
            var known = new HashSet<string>(items.Select(i => i.Path), StringComparer.OrdinalIgnoreCase);
            var missing = await ImportItems(paths.Where(m => !known.Contains(m))).ConfigureAwait(false);
            items.AddRange(missing);
            if (missing.Count > 0)
            {
                await SaveToFile(items).ConfigureAwait(false);
            }
        }

        public async Task Save(List<AssetCatalogItem> items)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                cachedList = items;

                await SaveToFile(items).ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task SaveToFile(List<AssetCatalogItem> items)
        {
            using var stream = File.Create(AssetsCatalogPath);
            await JsonSerializer.SerializeAsync(stream, items, new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter() } }).ConfigureAwait(false);
        }

        public Task<List<AssetCatalogItem>> ImportItems(IEnumerable<string> paths, string modId = "")
        {
            var result = new List<AssetCatalogItem>();
            foreach(var path in paths)
            {
                try
                {
                    var infos = arma3DataModule.Library.ReadModelInfoOnly(path);
                    if (infos != null)
                    {
                        result.Add(new AssetCatalogItem(path,
                            GetModId(modId, path),
                            DetectModel(infos, path),
                            infos.BboxMax.Vector3 - infos.BboxMin.Vector3,
                            (infos.BoundingCenter.Vector3 + infos.BboxMax.Vector3).Y,
                            infos.BboxMin.Vector3,
                            infos.BboxMax.Vector3,
                            infos.BoundingCenter.Vector3,
                            arma3DataModule.ProjectDrive.GetLastWriteTimeUtc(path)));
                    }
                }
                catch(Exception ex)
                {
                    logger.Warn(ex, "Import of {0} from {1} failed.", path, modId);
                }
            }
            return Task.FromResult(result);
        }

        private string GetModId(string modId, string path)
        {
            if (string.IsNullOrEmpty(modId))
            {
                if (path.StartsWith("a3\\", StringComparison.OrdinalIgnoreCase))
                {
                    if (path.Contains("f_enoch\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Arma 3: Livonia";
                    }
                    if (path.Contains("f_exp\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "Arma 3: Tanoa";
                    }
                    return "Arma 3";
                }
                if (path.StartsWith("ca\\", StringComparison.OrdinalIgnoreCase) || path.StartsWith("CUP\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "CUP";
                }
                if (path.StartsWith("z\\arm\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "ARM";
                }
                if (path.StartsWith("jbad", StringComparison.OrdinalIgnoreCase))
                {
                    return "JBAD";
                }
            }
            return modId;
        }

        private static AssetCatalogCategory DetectModel(IModelInfo modelInfo, string name)
        {
            if (modelInfo.MapType != MapType.Hide)
            {
                return ToCategory(modelInfo, name);
            }
            if (string.IsNullOrEmpty(modelInfo.Class))
            {
                logger.Debug($"No clue for '{name}'");
                return AssetCatalogCategory.Unknown;
            }
            return ClassToCategory(modelInfo.Class, name);
        }
        private static AssetCatalogCategory ClassToCategory(string modelClass, string name)
        {
            switch (modelClass.ToLowerInvariant())
            {
                case "treesoft":
                case "treehard":
                case "tree":
                    return AssetCatalogCategory.Tree;
                case "bushsoft":
                case "bush":
                    return AssetCatalogCategory.Bush;
                case "pond":
                    return AssetCatalogCategory.WaterSurface;
                case "housesimulated":
                case "building":
                case "house":
                    return AssetCatalogCategory.Building;
            }
            logger.Debug($"Unknown class='{modelClass}' for '{name}'");
            return AssetCatalogCategory.Unknown;
        }

        private static AssetCatalogCategory ToCategory(IModelInfo modelInfo, string name)
        {
            switch (modelInfo.MapType)
            {
                case MapType.Tree:
                case MapType.SmallTree:
                case MapType.Forest:
                    return AssetCatalogCategory.Tree;
                case MapType.Bush:
                    return AssetCatalogCategory.Bush;
                case MapType.Building:
                case MapType.House:
                case MapType.Tourism:
                    return AssetCatalogCategory.Building;
                case MapType.Church:
                case MapType.Chapel:
                    return AssetCatalogCategory.Church;
                case MapType.Cross:
                    return AssetCatalogCategory.Cross;
                case MapType.Rock:
                case MapType.Rocks:
                    return AssetCatalogCategory.Rock;
                case MapType.Bunker:
                    break;
                case MapType.Fortress:
                    break;
                case MapType.Fountain:
                    break;
                case MapType.ViewTower:
                    break;
                case MapType.Lighthouse:
                    return AssetCatalogCategory.Lighthouse;
                case MapType.Quay:
                    break;
                case MapType.Fuelstation:
                    break;
                case MapType.Hospital:
                    break;
                case MapType.Fence:
                case MapType.Wall:
                    return AssetCatalogCategory.FenceOrWall;
                case MapType.BusStop:
                    return AssetCatalogCategory.BusStop;
                case MapType.Transmitter:
                    return AssetCatalogCategory.RadioTower;
                case MapType.Stack:
                    break;
                case MapType.Ruin:
                    return AssetCatalogCategory.Ruins;
                case MapType.Watertower:
                    return AssetCatalogCategory.Watertower;
                case MapType.PowerLines:
                    return AssetCatalogCategory.PowerLines;
                case MapType.RailWay:
                    return AssetCatalogCategory.RailWay;
                case MapType.SolarPowerPlant:
                case MapType.WavePowerPlant:
                case MapType.WindPowerPlant:
                    return AssetCatalogCategory.PowerPlant;
                case MapType.River:
                    return AssetCatalogCategory.WaterSurface;
                case MapType.Road:
                case MapType.MainRoad:
                case MapType.Track:
                    return AssetCatalogCategory.BridgeOrRoad;
            }
            logger.Debug($"Unknown map='{modelInfo.MapType}' for '{name}'");
            return AssetCatalogCategory.Unknown;
        }

        public async Task<Dictionary<string, AssetCatalogItem>> GetItems(IEnumerable<string> paths)
        {
            var items = await GetOrLoad().ConfigureAwait(false);

            await EnsurePaths(items, paths).ConfigureAwait(false);

            var requestedItems = new Dictionary<string, AssetCatalogItem>(StringComparer.OrdinalIgnoreCase);
            var allItems = items.ToDictionary(k => k.Path, k => k, StringComparer.OrdinalIgnoreCase);
            var upgraded = false;
            foreach(var path in paths)
            {
                if (allItems.TryGetValue(path, out var item))
                {
                    if (!item.HasBounding || IsObsolete(item))
                    {
                        var infos = arma3DataModule.Library.ReadModelInfoOnly(path);
                        if (infos != null)
                        {
                            item.BboxMin = infos.BboxMin.Vector3;
                            item.BboxMax = infos.BboxMax.Vector3;
                            item.BoundingCenter = infos.BoundingCenter.Vector3;
                            item.Timestamp = arma3DataModule.ProjectDrive.GetLastWriteTimeUtc(path);
                            upgraded = true;
                            logger.Warn("Model {0} has been updated.", path);
                        }
                        else
                        {
                            logger.Warn("Model {0} is unresolved.", path);
                        }
                    }
                    requestedItems[path] = item;
                }
            }
            if (upgraded)
            {
                await SaveToFile(items).ConfigureAwait(false);
            }
            return requestedItems;
        }

        private bool IsObsolete(AssetCatalogItem item)
        {
            if (item.Timestamp == null)
            {
                return true;
            }
            var timestamp = arma3DataModule.ProjectDrive.GetLastWriteTimeUtc(item.Path);
            if (timestamp != null)
            {
                return timestamp.Value > item.Timestamp.Value;
            }
            return false;
        }
    }
}
