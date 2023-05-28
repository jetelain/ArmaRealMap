using System.Numerics;
using System.Text.Json;
using ArmaRealMap;
using ArmaRealMap.Configuration;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Core.Roads;
using ArmaRealMap.Libraries;
using ArmaRealMap.TerrainData.Roads;
using ArmaRealMapWebSite.Entities;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ModelInfoLibrary = GameRealisticMap.Arma3.TerrainBuilder.ModelInfoLibrary;
using OldModelInfoLibrary = ArmaRealMap.ModelInfoLibrary;

namespace ArmToGrmA3
{
    /// <summary>
    /// Utility program to convert ArmaRealMap data to GameRealisticMap.Arma3 data
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ExtractScreenShotsFromDB("c:\\temp\\");
            await ConvertToAssetConfiguration("c:\\temp\\");
        }

        private static async Task ExtractScreenShotsFromDB(string path)
        {
            var dbfile = Path.Combine(path, "assets.db");
            if (!File.Exists(dbfile))
            {
                return;
            }
            var assetsDB = new AssetsContext(Path.Combine(path, "assets.db"));
            var previews = await assetsDB.AssetPreviews.Where(p => p.Height == 1080).Include(p => p.Asset).ToListAsync();
            foreach (var preview in previews)
            {
                Console.WriteLine(preview.Asset.ModelPath);
                var targetFile = Path.Combine(path, "previews", Path.ChangeExtension(preview.Asset.ModelPath.ToLowerInvariant(), ".jpg"));
                using var image = Image.Load(preview.Data);
                image.Mutate(i => i.Resize(455, 256));
                image.Mutate(i => i.Contrast(1.4f)); // Screens shots was taken with wrong settings, fix constrast to match ingame editor previews colors
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                await image.SaveAsJpegAsync(targetFile, new JpegEncoder() { Quality = 95 });
            }
        }

        private static async Task ConvertToAssetConfiguration(string targetPath)
        {
            var globalConfig = ConfigLoader.LoadGlobal(string.Empty);
            ConfigLoader.SyncLibraries(globalConfig);
            var prj = new ProjectDrive(Arma3ToolsHelper.GetProjectDrivePath(), new PboFileSystem());
            var newModels = new ModelInfoLibrary(prj);
            await ConvertToAssetConfiguration(globalConfig, newModels, targetPath, TerrainRegion.CentralEurope);
            await ConvertToAssetConfiguration(globalConfig, newModels, targetPath, TerrainRegion.Sahel);
            Console.WriteLine($"Done");
        }

        private static async Task ConvertToAssetConfiguration(GlobalConfig globalConfig, ModelInfoLibrary newModels, string targetPath, TerrainRegion terrain)
        {
            Console.WriteLine($"Convert {terrain}");
            var data = Convert(terrain, globalConfig, newModels);
            Console.WriteLine($"Write {terrain}.grma3a");
            using (var stream = File.Create(Path.Combine(targetPath, terrain + ".grma3a")))
            {
                await JsonSerializer.SerializeAsync(stream, data, Arma3Assets.CreateJsonSerializerOptions(newModels));
            }
        }

        private static Arma3Assets Convert(TerrainRegion region, GlobalConfig globalConfig, ModelInfoLibrary newModels)
        {
            var newAssets = new Arma3Assets();
            ConvertRoads(region, globalConfig, newAssets);
            ConvertMaterials(region, globalConfig, newAssets);
            ConvertLibraries(region, globalConfig, newAssets, newModels);
            ImportPonds(region, newAssets, newModels);
            return newAssets;
        }

        private static void ConvertLibraries(TerrainRegion region, GlobalConfig globalConfig, Arma3Assets newAssets, ModelInfoLibrary newModels)
        {
            var olibs = new ObjectLibraries(region);
            olibs.Load(globalConfig.LibrariesFile);
            var oldModels = new OldModelInfoLibrary();

            oldModels.Load(globalConfig.ModelsInfoFile);
            newAssets.Objects.Add(ObjectTypeId.Tree, ConvertObject(olibs.GetSingleLibrary(ObjectCategory.IsolatedTree), newModels, oldModels));
            newAssets.Objects.Add(ObjectTypeId.Bench, ConvertObject(olibs.GetSingleLibrary(ObjectCategory.Bench), newModels, oldModels));
            newAssets.Objects.Add(ObjectTypeId.PicnicTable, ConvertObject(olibs.GetSingleLibrary(ObjectCategory.PicnicTable), newModels, oldModels));
            newAssets.Objects.Add(ObjectTypeId.WaterWell, ConvertObject(olibs.GetSingleLibrary(ObjectCategory.WaterWell), newModels, oldModels));
            newAssets.Objects = newAssets.Objects.OrderBy(o => o.Key).ToDictionary(p=>p.Key, p => p.Value);

            newAssets.Buildings.Add(BuildingTypeId.Residential, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Residential), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.Industrial, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Industrial), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.Retail, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Retail), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.Church, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Church), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.HistoricalFort, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.HistoricalFort), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.Hut, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Hut), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.Military, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.Military), newModels, oldModels));
            newAssets.Buildings.Add(BuildingTypeId.RadioTower, ConvertBuilding(olibs.GetSingleLibrary(ObjectCategory.RadioTower), newModels, oldModels));
            newAssets.Buildings = newAssets.Buildings.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);

            newAssets.Bridges.Add(RoadTypeId.TwoLanesMotorway, ConvertBridge(olibs.GetSingleLibrary(ObjectCategory.BridgePrimaryRoad), newModels, oldModels));
            newAssets.Bridges.Add(RoadTypeId.TwoLanesPrimaryRoad, ConvertBridge(olibs.GetSingleLibrary(ObjectCategory.BridgePrimaryRoad), newModels, oldModels));
            newAssets.Bridges.Add(RoadTypeId.TwoLanesSecondaryRoad, ConvertBridge(olibs.GetSingleLibrary(ObjectCategory.BridgeSecondaryRoad), newModels, oldModels));
            newAssets.Bridges.Add(RoadTypeId.TwoLanesConcreteRoad, ConvertBridge(olibs.GetSingleLibrary(ObjectCategory.BridgeConcreteRoad), newModels, oldModels));
            newAssets.Bridges = newAssets.Bridges.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);

            newAssets.BasicCollections.Add(BasicCollectionId.ScrubAdditional, ConvertBasic(olibs.GetSingleLibrary(ObjectCategory.ScrubAdditionalObjects), newModels, oldModels));
            newAssets.BasicCollections.Add(BasicCollectionId.ForestAdditional, ConvertBasic(olibs.GetSingleLibrary(ObjectCategory.ForestAdditionalObjects), newModels, oldModels));
            newAssets.BasicCollections.Add(BasicCollectionId.Rocks, ConvertBasic(olibs.GetSingleLibrary(ObjectCategory.GroundRock), newModels, oldModels));
            newAssets.BasicCollections.Add(BasicCollectionId.RocksAdditional, ConvertBasic(olibs.GetSingleLibrary(ObjectCategory.GroundRockAdditionalObjects), newModels, oldModels));
            newAssets.BasicCollections.Add(BasicCollectionId.DefaultAreas, ConvertBasic(olibs.GetSingleLibrary(ObjectCategory.RandomVegetation), newModels, oldModels));
            newAssets.BasicCollections = newAssets.BasicCollections.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);

            newAssets.ClusterCollections.Add(ClusterCollectionId.Forest, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.ForestTree), newModels, oldModels));
            newAssets.ClusterCollections.Add(ClusterCollectionId.ForestRadial, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.ForestRadialEdge), newModels, oldModels));
            newAssets.ClusterCollections.Add(ClusterCollectionId.ForestEdge, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.ForestEdge), newModels, oldModels));
            newAssets.ClusterCollections.Add(ClusterCollectionId.Scrub, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.Scrub), newModels, oldModels));
            newAssets.ClusterCollections.Add(ClusterCollectionId.ScrubRadial, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.ScrubRadialEdge), newModels, oldModels));
            newAssets.ClusterCollections.Add(ClusterCollectionId.WatercourseRadial, ConvertCluster(olibs.GetSingleLibrary(ObjectCategory.WaterwayBorder), newModels, oldModels));
            newAssets.ClusterCollections = newAssets.ClusterCollections.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);

            newAssets.Fences.Add(FenceTypeId.Wall, ConvertFences(olibs.GetLibraries(ObjectCategory.Wall), newModels, oldModels));
            newAssets.Fences.Add(FenceTypeId.Fence, ConvertFences(olibs.GetLibraries(ObjectCategory.Fence), newModels, oldModels));
            newAssets.Fences = newAssets.Fences.OrderBy(o => o.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        private static List<FenceDefinition> ConvertFences(List<ObjectLibrary> objectLibraries, ModelInfoLibrary newModels, OldModelInfoLibrary oldModels)
        {
            var fences = new List<FenceDefinition>();
            if (objectLibraries.Count > 0)
            {
                var defProbability = 1d / objectLibraries.Count;
                var sumExistingProbability = objectLibraries.Select(o => o.Probability ?? defProbability).Sum();
                foreach (var lib in objectLibraries)
                {
                    var probability = lib.Probability ?? defProbability;
                    fences.Add( new FenceDefinition(probability, ConvertToSegments(lib.Objects,newModels, oldModels)));
                }
            }
            fences.CheckProbabilitySum();
            return fences;
        }

        private static List<StraightSegmentDefinition> ConvertToSegments(List<SingleObjetInfos> objects, ModelInfoLibrary models, OldModelInfoLibrary oldModels)
        {
            var defProbability = 1d / objects.Count;
            var sumExistingProbability = objects.Select(o => o.PlacementProbability ?? defProbability).Sum();
            var items = new List<StraightSegmentDefinition>();
            foreach (var old in objects)
            {
                var model = models.ResolveByPath(oldModels.ResolveByName(old.Name).Path);
                var place = ObjectPlacementDetectedInfos.CreateFromODOL(models.ReadODOL(model.Path)!)!;
                var composition = GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, -place.GeneralRadius.Center);
                var probability = (old.PlacementProbability ?? defProbability) / sumExistingProbability;
                items.Add(new StraightSegmentDefinition(composition, (old.PlacementRadius ?? place.GeneralRadius.Radius) * 2));
            }
            return items;
        }

        private static List<BasicCollectionDefinition> ConvertBasic(ObjectLibrary? oldLibrary, ModelInfoLibrary models, OldModelInfoLibrary oldModels)
        {
            var items = new List<ClusterItemDefinition>();
            if (oldLibrary != null)
            {
                var defProbability = 1d / oldLibrary.Objects.Count;
                var sumExistingProbability = oldLibrary.Objects.Select(o => o.PlacementProbability ?? defProbability).Sum();
                foreach (var old in oldLibrary.Objects)
                {
                    var model = models.ResolveByPath(oldModels.ResolveByName(old.Name).Path);
                    var place = ObjectPlacementDetectedInfos.CreateFromODOL(models.ReadODOL(model.Path)!)!;
                    var composition = GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, -place.GeneralRadius.Center);
                    var probability = (old.PlacementProbability ?? defProbability) / sumExistingProbability;
                    items.Add(new ClusterItemDefinition(
                        old.PlacementRadius ?? place.GeneralRadius.Radius,
                        old.ReservedRadius ?? old.PlacementRadius ?? place.GeneralRadius.Radius,
                        composition, old.MaxZ, old.MinZ, probability, null, null));
                }
            }
            if (items.Count == 0)
            {
                return new List<BasicCollectionDefinition>();
            }
            return new List<BasicCollectionDefinition>() { new BasicCollectionDefinition(items, 1, oldLibrary?.Density ?? 0, oldLibrary?.Density ?? 0) };
        }

        private static List<ClusterCollectionDefinition> ConvertCluster(ObjectLibrary? oldLibrary, ModelInfoLibrary models, OldModelInfoLibrary oldModels)
        {
            var items = new List<ClusterDefinition>();
            var density = 0d;
            if (oldLibrary != null)
            {
                var defProbability = 1d / oldLibrary.Objects.Count;
                var sumExistingProbability = oldLibrary.Objects.Select(o => o.PlacementProbability ?? defProbability).Sum();
                foreach (var old in oldLibrary.Objects)
                {
                    var model = models.ResolveByPath(oldModels.ResolveByName(old.Name).Path);
                    var place = ObjectPlacementDetectedInfos.CreateFromODOL(models.ReadODOL(model.Path)!)!;
                    var composition = GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, -place.GeneralRadius.Center);
                    var probability = (old.PlacementProbability ?? defProbability) / sumExistingProbability;
                    items.Add(new ClusterDefinition(new ClusterItemDefinition(
                        old.PlacementRadius ?? place.GeneralRadius.Radius,
                        old.ReservedRadius ?? old.PlacementRadius ?? place.GeneralRadius.Radius,
                        composition, old.MaxZ, old.MinZ, 1, null, null), probability));
                }

                density = oldLibrary?.Density ?? 0.01d;
                var maxDensity = DensityHelper.GetMaxDensity(items);
                if (density > maxDensity)
                {
                    density = maxDensity;
                }
            }
            if (items.Count == 0)
            {
                return new List<ClusterCollectionDefinition>();
            }
            return new List<ClusterCollectionDefinition>() { new ClusterCollectionDefinition(items, 1, density, density) };
        }

        private static BridgeDefinition ConvertBridge(ObjectLibrary objectLibrary, ModelInfoLibrary newModels, OldModelInfoLibrary oldModels)
        {
            return new BridgeDefinition(
                ConvertBridgeSegment(objectLibrary.Objects[0], newModels, oldModels),
                ConvertBridgeSegment(objectLibrary.Objects[1], newModels, oldModels),
                ConvertBridgeSegment(objectLibrary.Objects[2], newModels, oldModels),
                ConvertBridgeSegment(objectLibrary.Objects[3], newModels, oldModels));
        }

        private static StraightSegmentDefinition ConvertBridgeSegment(SingleObjetInfos single, ModelInfoLibrary newModels, OldModelInfoLibrary oldModels)
        {
            var model = newModels.ResolveByPath(oldModels.ResolveByName(single.Name).Path);
            return new StraightSegmentDefinition(GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, new Vector2(-single.CX, -single.CY), single.CZ), single.Depth);
        }

        private static void ImportPonds(TerrainRegion region, Arma3Assets newAssets, ModelInfoLibrary newModels)
        {
            if (region == TerrainRegion.Sahel)
            {
                newAssets.Ponds.Add(PondSizeId.Size5, newModels.ResolveByPath(@"z\arm\addons\sahel\data\water\arm_pond_5.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size10, newModels.ResolveByPath(@"z\arm\addons\sahel\data\water\arm_pond_10.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size20, newModels.ResolveByPath(@"z\arm\addons\sahel\data\water\arm_pond_20.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size40, newModels.ResolveByPath(@"z\arm\addons\sahel\data\water\arm_pond_40.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size80, newModels.ResolveByPath(@"z\arm\addons\sahel\data\water\arm_pond_80.p3d"));
            }
            else
            {
                newAssets.Ponds.Add(PondSizeId.Size5, newModels.ResolveByPath(@"z\arm\addons\common_v2\data\water\arm_pond_blue_5.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size10, newModels.ResolveByPath(@"z\arm\addons\common_v2\data\water\arm_pond_blue_10.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size20, newModels.ResolveByPath(@"z\arm\addons\common_v2\data\water\arm_pond_blue_20.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size40, newModels.ResolveByPath(@"z\arm\addons\common_v2\data\water\arm_pond_blue_40.p3d"));
                newAssets.Ponds.Add(PondSizeId.Size80, newModels.ResolveByPath(@"z\arm\addons\common_v2\data\water\arm_pond_blue_80.p3d"));
            }
        }

        private static List<BuildingDefinition> ConvertBuilding(ObjectLibrary oldLibrary, ModelInfoLibrary models, OldModelInfoLibrary oldModels)
        {
            var result = new List<BuildingDefinition>();
            if (oldLibrary != null)
            {
                foreach (var old in oldLibrary.Objects)
                {
                    var model = models.ResolveByPath(oldModels.ResolveByName(old.Name).Path);
                    var place = ObjectPlacementDetectedInfos.CreateFromModel(model, models)!;
                    if (place != null)
                    {
                        var composition = GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, -place.UpperRectangle.Center);
                        result.Add(new BuildingDefinition(place.UpperRectangle.Size, composition));
                    }
                    else
                    {
                        Console.Error.WriteLine($"{model.Path} is ignored, it has no Geometry LOD");
                    }
                }

                // TODO: Compositions
            }
            return result;
        }

        private static List<ObjectDefinition> ConvertObject(ObjectLibrary oldLibrary, ModelInfoLibrary models, OldModelInfoLibrary oldModels)
        {
            var result = new List<ObjectDefinition>();
            if (oldLibrary != null)
            {
                var defProbability = 1d / oldLibrary.Objects.Count;
                var sumExistingProbability = oldLibrary.Objects.Select(o => o.PlacementProbability ?? defProbability).Sum();
                foreach(var old in oldLibrary.Objects)
                {
                    var model = models.ResolveByPath(oldModels.ResolveByName(old.Name).Path);
                    var place = ObjectPlacementDetectedInfos.CreateFromModel(model, models)!;
                    var composition = GameRealisticMap.Arma3.Assets.Composition.CreateSingleFrom(model, -place.TrunkRadius.Center);
                    result.Add(new ObjectDefinition(composition, (old.PlacementProbability ?? defProbability) / sumExistingProbability));
                }
            }
            result.CheckProbabilitySum();
            return result;
        }

        private static void ConvertMaterials(TerrainRegion region, GlobalConfig globalConfig, Arma3Assets newAssets)
        {
            var mats = new Dictionary<TerrainMaterial, List<TerrainMaterialUsage>>();
            var terrainMaterialLibrary = new ArmaRealMap.TerrainData.GroundDetailTextures.TerrainMaterialLibrary();
            terrainMaterialLibrary.LoadFromFile(globalConfig.TerrainMaterialFile, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.FarmLand, TerrainMaterialUsage.FarmLand, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Default, TerrainMaterialUsage.Default, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Dirt, TerrainMaterialUsage.DefaultIndustrial, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Default, TerrainMaterialUsage.DefaultUrban, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Forest, TerrainMaterialUsage.ForestGround, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.GrassShort, TerrainMaterialUsage.Grass, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.WetLand, TerrainMaterialUsage.LakeGround, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.GrassShort, TerrainMaterialUsage.Meadow, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.WetLand, TerrainMaterialUsage.RiverGround, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Sand, TerrainMaterialUsage.Sand, mats, region);
            MapMaterial(terrainMaterialLibrary, ArmaRealMap.GroundTextureDetails.TerrainMaterial.Rock, TerrainMaterialUsage.RockGround, mats, region);
            newAssets.Materials = new TerrainMaterialLibrary(mats.Select(pair => new TerrainMaterialDefinition(pair.Key, pair.Value.OrderBy(m => m).ToArray())).OrderBy(m => m.Usages.Min()).ToList());
        }

        private static void MapMaterial(ArmaRealMap.TerrainData.GroundDetailTextures.TerrainMaterialLibrary oldLib, ArmaRealMap.GroundTextureDetails.TerrainMaterial oldId, TerrainMaterialUsage usage, Dictionary<TerrainMaterial, List<TerrainMaterialUsage>> newMaterials, TerrainRegion region)
        {
            var oldMaterial = oldLib.GetInfo(oldId, region);

            if (oldMaterial.ColorTexture.Contains("_centraleurope_"))
            {
                oldMaterial.ColorTexture = oldMaterial.ColorTexture.Replace("\\common\\", "\\centraleurope\\");
                oldMaterial.NormalTexture = oldMaterial.NormalTexture.Replace("\\common\\", "\\centraleurope\\");
            }
            else if (oldMaterial.ColorTexture.Contains("_sahel_"))
            {
                oldMaterial.ColorTexture = oldMaterial.ColorTexture.Replace("\\common\\", "\\sahel\\");
                oldMaterial.NormalTexture = oldMaterial.NormalTexture.Replace("\\common\\", "\\sahel\\");
            }
            else
            {
                oldMaterial.ColorTexture = oldMaterial.ColorTexture.Replace("\\common\\", "\\common_v2\\");
                oldMaterial.NormalTexture = oldMaterial.NormalTexture.Replace("\\common\\", "\\common_v2\\");
            }
            var newMaterial = newMaterials.Keys.FirstOrDefault(m => string.Equals(m.ColorTexture, oldMaterial.ColorTexture, StringComparison.OrdinalIgnoreCase));
            if (newMaterial == null)
            {
                newMaterial = new TerrainMaterial(oldMaterial.NormalTexture, oldMaterial.ColorTexture, oldId.GetColor(region).ToPixel<Rgb24>(), oldMaterial.FakeSatPngImage);
                newMaterials.Add(newMaterial, new List<TerrainMaterialUsage>());
            }
            newMaterials[newMaterial].Add(usage);
        }

        private static void ConvertRoads(TerrainRegion region, GlobalConfig globalConfig, Arma3Assets newAssets)
        {
            var rlib = new RoadTypeLibrary();
            rlib.LoadFromFile(globalConfig.RoadTypesFile, region);
            var legacyRoadTypes = Enum.GetValues<RoadTypeId>().Where(r => r != RoadTypeId.ConcreteFootway && r != RoadTypeId.SingleLaneConcreteRoad);
            newAssets.Roads.AddRange(legacyRoadTypes.Select(id => ConvertRoad(rlib.GetInfo(id, region))));
            newAssets.Roads.Add(ExtrapolateRoad(rlib.GetInfo(RoadTypeId.TwoLanesConcreteRoad, region), RoadTypeId.SingleLaneConcreteRoad));
            newAssets.Roads.Add(ExtrapolateRoad(rlib.GetInfo(RoadTypeId.TwoLanesConcreteRoad, region), RoadTypeId.ConcreteFootway));
            newAssets.Roads.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static Arma3RoadTypeInfos ExtrapolateRoad(RoadTypeInfos source, RoadTypeId id)
        {
            var def = new DefaultRoadTypeLibrary().GetInfo(id);
            return new Arma3RoadTypeInfos(id, 
                Color.Parse(source.SatelliteColor),
                def.Width * source.TextureWidth / source.TextureWidth,
                source.Texture, 
                source.TextureEnd,
                source.Material,
                def.Width,
                def.ClearWidth);
        }

        private static Arma3RoadTypeInfos ConvertRoad(RoadTypeInfos roadTypeInfos)
        {
            return new Arma3RoadTypeInfos(roadTypeInfos.Id, Color.Parse(roadTypeInfos.SatelliteColor), roadTypeInfos.TextureWidth, roadTypeInfos.Texture, roadTypeInfos.TextureEnd, roadTypeInfos.Material, roadTypeInfos.Width, roadTypeInfos.ClearWidth);
        }
    }
}