using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Arma3
{
    public class Arma3MapGenerator
    {
        private readonly IArma3RegionAssets assets;
        private readonly ProjectDrive projectDrive;

        public Arma3MapGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive)
        {
            this.assets = assets;
            this.projectDrive = projectDrive;
        }

        public async Task<IImagerySource?> GetImagerySource(IProgressTask progress, Arma3MapConfig a3config, IHugeImageStorage hugeImageStorage)
        {
            var context = await GetBuildContext(progress, a3config, hugeImageStorage); 
            if (context == null)
            {
                return null;
            }
            return new ImagerySource(assets.Materials, progress, projectDrive, a3config, context);
        }

        public async Task<IBuildContext?> GetBuildContext(IProgressTask progress, Arma3MapConfig a3config, IHugeImageStorage hugeImageStorage)
        {
            var loader = new OsmDataOverPassLoader(progress);
            var osmSource = await loader.Load(a3config.TerrainArea);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            var builders = new BuildersCatalog(progress, assets.RoadTypeLibrary);
            return new BuildContext(builders, progress, a3config.TerrainArea, osmSource, a3config.Imagery, hugeImageStorage);
        }

        [SupportedOSPlatform("windows")]
        public async Task GenerateMod(IProgressTask progress, Arma3MapConfig a3config)
        {
            progress.Total += 1;

            await GenerateWrp(progress, a3config);

            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // TODO: Unpack all missing P3D + PAA

            Directory.CreateDirectory(a3config.TargetModDirectory);
            await Arma3ToolsHelper.BuildWithMikeroPboProject(a3config.PboPrefix, a3config.TargetModDirectory, progress);
            progress.ReportOneDone();
        }

        public async Task<BuildContext?> GenerateWrp(IProgressTask progress, Arma3MapConfig a3config)
        {
            var generators = new Arma3LayerGeneratorCatalog(progress, assets);
            progress.Total += 5 + generators.Generators.Count;

            // Download from OSM
            var loader = new OsmDataOverPassLoader(progress);
            var osmSource = await loader.Load(a3config.TerrainArea);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Generate content
            var builders = new BuildersCatalog(progress, assets.RoadTypeLibrary);
            var context = new BuildContext(builders, progress, a3config.TerrainArea, osmSource, a3config.Imagery);
            GenerateWrp(progress, a3config, context, a3config.TerrainArea, generators); 
            context.DisposeHugeImages();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Convert PAA
            await projectDrive.ProcessImageToPaa(progress);
            progress.ReportOneDone();

            return context;
        }

        public void GenerateWrp(IProgressTask progress, IArma3MapConfig config, IContext context, ITerrainArea area, Arma3LayerGeneratorCatalog generators)
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

            // Imagery
            var imageryCompiler = new ImageryCompiler(assets.Materials, progress, projectDrive);

            var tiles = imageryCompiler.Compile(config, new ImagerySource(assets.Materials, progress, projectDrive, config, context));
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Objects + WRP
            var wrpBuilder = new WrpCompiler(progress, projectDrive);

            var grid = context.GetData<ElevationData>().Elevation;

            var size = area.SizeInMeters;

            var objects = generators.Generators
                .Progress(progress)
                .SelectMany(tb => tb.Generate(config, context))
                .Select(o => o.ToWrpObject(grid))
                .Where(o => IsStrictlyInside(o, size));

            wrpBuilder.Write(config, grid, tiles, objects);
            progress.ReportOneDone();
        }

        private bool IsStrictlyInside(EditableWrpObject o, float size)
        {
            return o.Transform.TranslateX >= 0 && o.Transform.TranslateX < size 
                && o.Transform.TranslateZ >= 0 && o.Transform.TranslateZ < size;
        }
    }
}
