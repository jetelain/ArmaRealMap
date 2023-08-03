using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
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
            progress.Total += 6 + generators.Generators.Count;

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
            using (var report = progress.CreateStepPercent("LayersCfg"))
            {
                new LayersCfgGenerator(assets.Materials, projectDrive).WriteLayersCfg(config);
                progress.ReportOneDone();
            }

            // Imagery
            var source = new ImagerySource(assets.Materials, progress, projectDrive, config, context);
            using (var idMap = source.CreateIdMap())
            {
                await WriteImage(idMap, Path.Combine(targetDirectory, "idmap.png")).ConfigureAwait(false);
            }
            using (var satMap = source.CreateSatMap())
            {
                await WriteImage(satMap, Path.Combine(targetDirectory, "stamap.png")).ConfigureAwait(false);
            }
            context.DisposeHugeImages();

            // TODO: CreatePictureMap/CreateSatOut

            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Elevation
            var grid = context.GetData<ElevationData>().Elevation.ToDataCell(new Coordinates(0, TerrainBuilderObject.XShift));
            using (var writer = File.CreateText(Path.Combine(targetDirectory,"elevation.asc")))
            {
                using var report = progress.CreateStepPercent("Elevation.AscFile");
                EsriAsciiHelper.SaveDataCell(writer, grid, "-9999", report);
            }
            progress.ReportOneDone();

            var usedModels = new HashSet<ModelInfo>();

            // Objects
            foreach (var tb in generators.Generators)
            {
                var name = tb.GetType().Name.Replace("Generator", "");
                var entries = tb.Generate(config, context).ToList();
                foreach (var entry in entries)
                {
                    usedModels.Add(entry.Model);
                }
                await WriteFileAsync(Path.Combine(targetDirectory, name + ".abs.txt"),entries.Where(e => e.ElevationMode == ElevationMode.Absolute).ToList());
                await WriteFileAsync(Path.Combine(targetDirectory, name + ".rel.txt"), entries.Where(e => e.ElevationMode == ElevationMode.Relative).ToList());
                progress.ReportOneDone();
            }

            new DependencyUnpacker(assets, projectDrive).Unpack(progress, usedModels.Select(m => m.Path));
            progress.ReportOneDone();

            // TODO: Create TML files

            await File.WriteAllTextAsync(Path.Combine(targetDirectory, "README.txt"), CreateReadMe(config));
        }

        private string? CreateReadMe(Arma3MapConfig config)
        {
            var tiler = new ImageryTiler(config);

            var textureLayerSize = config.SizeInMeters / WrpCompiler.LandRange(config.SizeInMeters);

            return  FormattableString.Invariant($@"# {config.WorldName}

## Mapframe properties
    Location
        Properties
            Name              = {config.WorldName}
            Ouput root folder = P:\{config.PboPrefix}

        Location
            Easting  = {TerrainBuilderObject.XShift}
            Northing = 0

    Sampler
        Terrain sampler
            Grid size        = {config.TerrainArea.GridSize} x {config.TerrainArea.GridSize}
            Cell size (m)    = {config.TerrainArea.GridCellSize}
            Terrain size (m) = {config.SizeInMeters}

        Satellite/Surface (mask) source images
            Size (px)         = {tiler.FullImageSize} x {tiler.FullImageSize}
            Resolution (m/px) = {config.Resolution}

        Satellite/Surface (mask) tiles
            Size (px)            = {tiler.TileSize} x {tiler.TileSize}
            Desired overlap (px) = 16

        Texture layer
            Size (m) = {textureLayerSize} x {textureLayerSize}

    Processing
        Layers config file (layers.cfg) = P:\{config.PboPrefix}\data\gdt\layers.cfg


## Objects

Files *.rel.txt have to be imported with relative elevation.

Files *.abs.txt have to be imported with absolute elevation.
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
