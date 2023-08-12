using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.P3D.ODOL;
using GameRealisticMap.Studio.Modules.Arma3Data;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    [Export(typeof(IAssetsCatalogService))]
    internal class AssetsCatalogService : IAssetsCatalogService
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("AssetsCatalogService");

        private readonly IArma3DataModule arma3DataModule;

        [ImportingConstructor]
        public AssetsCatalogService(IArma3DataModule arma3DataModule)
        {
            this.arma3DataModule = arma3DataModule;
        }

        public async Task<List<AssetCatalogItem>> LoadFrom(string fileName)
        {
            if ( File.Exists(fileName))
            {
                using var stream = File.OpenRead(fileName);
                return await JsonSerializer.DeserializeAsync<List<AssetCatalogItem>>(stream, new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter() } }) ?? new List<AssetCatalogItem>();
            }
            return new List<AssetCatalogItem>();
        }

        public async Task SaveTo(string fileName, List<AssetCatalogItem> items)
        {
            using var stream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(stream, items, new JsonSerializerOptions() { Converters = { new JsonStringEnumConverter() } });
        }

        public Task<List<AssetCatalogItem>> ImportItems(IEnumerable<string> paths, string modId = "")
        {
            var result = new List<AssetCatalogItem>();
            foreach(var path in paths)
            {
                try
                {
                    using var stream = arma3DataModule.ProjectDrive.OpenFileIfExists(path);
                    if (stream != null)
                    {
                        var infos = StreamHelper.Read<P3DInfosOnly>(stream);
                        result.Add(new AssetCatalogItem(path, GetModId(modId, path), DetectModel(infos.ModelInfo, path), infos.ModelInfo.BboxMax.Vector3 - infos.ModelInfo.BboxMin.Vector3, GetHeight(infos.ModelInfo)));
                    }
                }
                catch(Exception ex)
                {
                    logger.Warn(ex, "Import of {0} from {1} failed.", path, modId);
                }
            }
            return Task.FromResult(result);
        }

        private float GetHeight(IModelInfo modelInfo)
        {
            if ( modelInfo is ModelInfo odolModel)
            {
                return (odolModel.BoundingCenter.Vector3 + odolModel.BboxMax.Vector3).Y;
            }
            return modelInfo.BboxMax.Y;
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

        private AssetCatalogCategory DetectModel(IModelInfo modelInfo, string name)
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
        private AssetCatalogCategory ClassToCategory(string modelClass, string name)
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

        private AssetCatalogCategory ToCategory(IModelInfo modelInfo, string name)
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
    }
}
