using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BIS.Core.Streams;
using BIS.P3D;
using GameRealisticMap.Studio.Modules.Arma3Data;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    [Export(typeof(IAssetsCatalogService))]
    internal class AssetsCatalogService : IAssetsCatalogService
    {
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
                using var stream = arma3DataModule.ProjectDrive.OpenFileIfExists(path);
                if (stream != null)
                {
                    var infos = StreamHelper.Read<P3DInfosOnly>(stream);
                    if (DetectModel(infos.ModelInfo, out var category))
                    {
                        result.Add(new AssetCatalogItem(path, GetModId(modId, path), category));
                    }
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
                    return "Arma3";
                }
                if (path.StartsWith("ca\\", StringComparison.OrdinalIgnoreCase) || path.StartsWith("CUP\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "CUP";
                }
                if (path.StartsWith("z\\arm\\", StringComparison.OrdinalIgnoreCase))
                {
                    return "ARM";
                }
            }
            return modId;
        }

        private bool DetectModel(IModelInfo modelInfo, out AssetCatalogCategory category)
        {
            if (modelInfo.MapType != MapType.Hide)
            {
                category = ToCategory(modelInfo);
                return true;
            }
            category = AssetCatalogCategory.Unknown;
            if ( string.IsNullOrEmpty(modelInfo.Class))
            {
                return true;
            }
            return false;
        }

        private AssetCatalogCategory ToCategory(IModelInfo modelInfo)
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
                    return AssetCatalogCategory.Building;
                case MapType.Church:
                case MapType.Chapel:
                    return AssetCatalogCategory.Church;
                case MapType.Cross:
                    return AssetCatalogCategory.Cross;
                case MapType.Rock:
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
                    break;
                case MapType.Quay:
                    break;
                case MapType.Fuelstation:
                    break;
                case MapType.Hospital:
                    break;
                case MapType.Fence:
                    return AssetCatalogCategory.Fence;
                case MapType.Wall:
                    return AssetCatalogCategory.Wall;
                case MapType.BusStop:
                    return AssetCatalogCategory.BusStop;
                case MapType.Road:
                    break;
                case MapType.Transmitter:
                    break;
                case MapType.Stack:
                    break;
                case MapType.Ruin:
                    break;
                case MapType.Tourism:
                    break;
                case MapType.Watertower:
                    return AssetCatalogCategory.Watertower;
                case MapType.Track:
                    break;
                case MapType.MainRoad:
                    break;
                case MapType.Rocks:
                    break;
                case MapType.PowerLines:
                    break;
                case MapType.RailWay:
                    break;
                case MapType.SolarPowerPlant:
                    break;
                case MapType.WavePowerPlant:
                    break;
                case MapType.WindPowerPlant:
                    break;
                case MapType.River:
                    break;
                case MapType.Unkwown:
                    break;
                default:
                    break;
            }
            return AssetCatalogCategory.Unknown;
        }
    }
}
