﻿using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.TerrainBuilder.TmlFiles;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Preview;
using GameRealisticMap.Reporting;
using HugeImages;
using HugeImages.Processing;
using HugeImages.Storage;
using MapToolkit;
using MapToolkit.DataCells.FileFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3
{
    public class Arma3TerrainBuilderGenerator
    {
        private readonly IArma3RegionAssets assets;
        private readonly ProjectDrive projectDrive;

        public Arma3TerrainBuilderGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive)
        {
            this.assets = assets;
            this.projectDrive = projectDrive;
        }

        private BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var builders = new BuildersCatalog(progress, assets.RoadTypeLibrary, assets.Railways);
            return new BuildContext(builders, progress, a3config.TerrainArea, osmSource, a3config.Imagery, hugeImageStorage);
        }

        public async Task GenerateTerrainBuilderFiles(IProgressTask progress, Arma3MapConfig a3config, string targetDirectory)
        {
            var generators = new Arma3LayerGeneratorCatalog(progress, assets);
            progress.Total += 7 + generators.Generators.Count;

            // Download from OSM
            var osmSource = await LoadOsmData(progress, a3config);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Generate content
            var context = CreateBuildContext(progress, a3config, osmSource);
            await GenerateFilesAsync(progress, a3config, context, a3config.TerrainArea, generators, targetDirectory);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

#if DEBUG
            PreviewRender.RenderHtml(context, "debug.html").Wait();
#endif

            // Convert PAA
            await projectDrive.ProcessImageToPaa(progress);
            progress.ReportOneDone();

        }

        private async Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            var loader = new OsmDataOverPassLoader(progress);
            return await loader.Load(a3config.TerrainArea);
        }

        private async Task GenerateFilesAsync(IProgressTask progress, Arma3MapConfig config, BuildContext context, ITerrainArea area, Arma3LayerGeneratorCatalog generators, string targetDirectory)
        {
            // Game config
            new GameConfigGenerator(assets, projectDrive).Generate(config, context, area);

            // Roads
            var roadsCompiler = new RoadsCompiler(progress, projectDrive, assets.RoadTypeLibrary);

            roadsCompiler.Write(config, context.GetData<RoadsData>().Roads);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // layers.cfg
            CreateLayersCfg(progress, config);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Imagery
            await ExportImagery(progress, config, context, targetDirectory).ConfigureAwait(false);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Elevation
            ExportElevation(progress, context, targetDirectory);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Objects
            await ExportObjects(progress, config, context, generators, targetDirectory);

            await File.WriteAllTextAsync(Path.Combine(targetDirectory, "README.txt"), CreateReadMe(config, targetDirectory));
        }

        private async Task ExportObjects(IProgressTask progress, Arma3MapConfig config, BuildContext context, Arma3LayerGeneratorCatalog generators, string targetDirectory)
        {
            var usedModels = new HashSet<ModelInfo>();
            var objectsTargetDirectory = Path.Combine(targetDirectory, "Objects");
            Directory.CreateDirectory(objectsTargetDirectory);
            foreach (var tb in generators.Generators)
            {
                var name = tb.GetType().Name.Replace("Generator", "").ToLowerInvariant();
                var entries = tb.Generate(config, context).ToList();
                foreach (var entry in entries)
                {
                    usedModels.Add(entry.Model);
                }
                await WriteFileAsync(Path.Combine(objectsTargetDirectory, "absolute_" + name + ".txt"), entries.Where(e => e.ElevationMode == ElevationMode.Absolute).ToList());
                await WriteFileAsync(Path.Combine(objectsTargetDirectory, "relative_" + name + ".txt"), entries.Where(e => e.ElevationMode == ElevationMode.Relative).ToList());
                progress.ReportOneDone();
            }

            new DependencyUnpacker(assets, projectDrive).Unpack(progress, usedModels.Select(m => m.Path));
            progress.ReportOneDone();

            new TmlGenerator().WriteLibrariesTo(usedModels, Path.Combine(targetDirectory, "Library"));
        }

        private void CreateLayersCfg(IProgressTask progress, Arma3MapConfig config)
        {
            using (var report = progress.CreateStepPercent("LayersCfg"))
            {
                new LayersCfgGenerator(assets.Materials, projectDrive).WriteLayersCfg(config);
                progress.ReportOneDone();
            }

            if (OperatingSystem.IsWindows())
            {
                var legendSource = Path.Combine(Arma3ToolsHelper.GetArma3ToolsPath(), "TerrainBuilder\\img\\map\\maplegend.png");
                var legendTarget = projectDrive.GetFullPath("legend.png");
                if (File.Exists(legendSource) && !File.Exists(legendTarget))
                {
                    File.Copy(legendSource, projectDrive.GetFullPath(legendTarget), true);
                }
            }
        }

        private static void ExportElevation(IProgressTask progress, BuildContext context, string targetDirectory)
        {
            var grid = context.GetData<ElevationData>().Elevation.ToDataCell(new Coordinates(0, TerrainBuilderObject.XShift));
            using (var writer = File.CreateText(Path.Combine(targetDirectory, "elevation.asc")))
            {
                using var report = progress.CreateStepPercent("Elevation.AscFile");
                EsriAsciiHelper.SaveDataCell(writer, grid, "-9999", report);
            }
        }

        private async Task ExportImagery(IProgressTask progress, Arma3MapConfig config, BuildContext context, string targetDirectory)
        {
            var source = new ImagerySource(assets.Materials, progress, projectDrive, config, context);
            using (var idMap = source.CreateIdMap())
            {
                await WriteImage(idMap, Path.Combine(targetDirectory, "idmap.png")).ConfigureAwait(false);
            }
            ImageryCompiler.CreateConfigCppImages(projectDrive, config, source);
            using (var satMap = source.CreateSatMap())
            {
                await WriteImage(satMap, Path.Combine(targetDirectory, "satmap.png")).ConfigureAwait(false);
            }
            context.DisposeHugeImages();
        }

        private string? CreateReadMe(Arma3MapConfig config, string targetDirectory)
        {
            var tiler = new ImageryTiler(config);

            var textureLayerSize = config.SizeInMeters / WrpCompiler.LandRange(config.SizeInMeters);

            return  FormattableString.Invariant($@"# {config.WorldName}

See https://github.com/jetelain/ArmaRealMap/wiki/Terrain-Builder-Export for additional instructions.

Remember to mount project drive with Arma 3 Tools, or your custom tool, before launching Terrain Builder.

## Mapframe properties
    Location
        Properties
            Name               = {config.WorldName}
            Output root folder = P:\{config.PboPrefix}

        Location
            Easting  = {TerrainBuilderObject.XShift}
            Northing = 0

    Sampler
        Terrain sampler
            Grid size        = {config.TerrainArea.GridSize} x {config.TerrainArea.GridSize}
            Cell size (m)    = {config.TerrainArea.GridCellSize}
            Terrain size (m) = {config.SizeInMeters}

        Satellite/Surface (mask) source images
            Size (px)         = {tiler.FullImageSize.Width} x {tiler.FullImageSize.Height}
            Resolution (m/px) = {config.Resolution}

        Satellite/Surface (mask) tiles
            Size (px)            = {tiler.TileSize} x {tiler.TileSize}
            Desired overlap (px) = 16

        Texture layer
            Size (m) = {textureLayerSize} x {textureLayerSize} (see note 1)

    Processing
        Layers config file (layers.cfg) = P:\{config.PboPrefix}\data\gdt\layers.cfg


Note 1 : 
If you choose an other texture layer size, you will have to edit the rvmat files generated in 
'P:\{config.PboPrefix}\data\gdt' to keep a correct in game texture size.

## Elevation

Import '{Path.Combine(targetDirectory, "elevation.asc")}' with 'File > Import > Terrains...'.

## Imagery

Import '{Path.Combine(targetDirectory, "satmap.png")}' with 'File > Import > Satellite images...'.

Import '{Path.Combine(targetDirectory, "idmap.png")}' with 'File > Import > Surface mask images...'.

For both import, use following Localisation Options :

    Left-Bottom location
        Easting  = {TerrainBuilderObject.XShift}
        Northing = 0
    
    Resolution
        X (m/px) = {config.Resolution}
        Y (m/px) = {config.Resolution}

For large images this operation can take several minutes.

## Libraries

You may use your own libraries, or import tml files from '{Path.Combine(targetDirectory, "Library")}'.

If you use your own libraries, import at least 'arm.tml' if present.

To import files, go into 'Library manager', then right click on root element, then 'Load library'.

## Objects

Import objects from '{Path.Combine(targetDirectory, "Objects")}' with 'File > Import > Objects...'.

Files relative_*.txt have to be imported with relative elevation.

Files absolute_*.txt have to be imported with absolute elevation.
");
        }

        private static async Task WriteImage(HugeImage<Rgba32> idMap, string filename)
        {
            using (var bigImage = new Image<Rgb24>(idMap.Size.Width, idMap.Size.Height))
            {
                // TODO: Build smaller images if size is too big

                await bigImage.MutateAsync(async i =>
                {
                    await i.DrawHugeImageAsync(idMap, Point.Empty).ConfigureAwait(false);

                }).ConfigureAwait(false);

                await bigImage.SaveAsPngAsync(filename).ConfigureAwait(false);
            }
        }

        private async Task WriteFileAsync(string filePath, List<TerrainBuilderObject> terrainBuilderObjects)
        {
            if (terrainBuilderObjects.Count > 0)
            {
                await File.WriteAllLinesAsync(filePath, terrainBuilderObjects.Select(o => o.ToTerrainBuilderCSV()));
            }
        }

    }
}