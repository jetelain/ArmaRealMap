using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Preview;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Arma3
{
    public class Arma3MapGenerator
    {
        protected readonly IArma3RegionAssets assets;
        private readonly ProjectDrive projectDrive;
        private readonly IPboCompilerFactory pboCompilerFactory;
        protected readonly ISourceLocations sources;

        public Arma3MapGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive, IPboCompilerFactory pboCompilerFactory, ISourceLocations sources)
        {
            this.assets = assets;
            this.projectDrive = projectDrive;
            this.pboCompilerFactory = pboCompilerFactory;
            this.sources = sources;
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
            var osmSource = await LoadOsmData(progress, a3config);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            return CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);
        }

        protected virtual BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var builders = new BuildersCatalog(progress, assets, sources);
            return new BuildContext(builders, progress, a3config.TerrainArea, osmSource, a3config.Imagery, hugeImageStorage);
        }

        [SupportedOSPlatform("windows")]
        public async Task<string?> GenerateMod(IProgressTask progress, Arma3MapConfig a3config)
        {
            progress.Total += 1;

            var results = await GenerateWrp(progress, a3config);
            if (results == null || progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            Directory.CreateDirectory(a3config.TargetModDirectory);
            await pboCompilerFactory.Create(progress).BinarizeAndCreatePbo(a3config, results.UsedModels, results.UsedRvmat);
            progress.ReportOneDone();

            if (results == null || progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            var name = GameConfigGenerator.GetFreindlyName(a3config, results.Context.GetData<CitiesData>());
            var modCpp = Path.Combine(a3config.TargetModDirectory, "mod.cpp");
            if (!File.Exists(modCpp))
            {
                File.WriteAllText(modCpp, $"name = \"{name}, GameRealisticMap\";");
            }
            return name;
        }

        public async Task<WrpAndContextResults?> GenerateWrp(IProgressTask progress, Arma3MapConfig a3config, bool pngToPaa = true)
        {
            var generators = new Arma3LayerGeneratorCatalog(progress, assets);
            progress.Total += 6 + generators.Generators.Count;

            // Download from OSM
            var osmSource = await LoadOsmData(progress, a3config);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Generate content
            var context = CreateBuildContext(progress, a3config, osmSource);
            var results = GenerateWrp(progress, a3config, context, a3config.TerrainArea, generators);
            context.DisposeHugeImages();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Convert PAA
            if (pngToPaa)
            {
                await projectDrive.ProcessImageToPaa(progress);
            }
            progress.ReportOneDone();

            return results;
        }

        protected virtual async Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            var loader = new OsmDataOverPassLoader(progress, sources);
            return await loader.Load(a3config.TerrainArea);
        }

        public WrpAndContextResults? GenerateWrp(IProgressTask progress, Arma3MapConfig config, IContext context, ITerrainArea area, Arma3LayerGeneratorCatalog generators)
        {
            // Game config
            new GameConfigGenerator(assets, projectDrive).Generate(config, context, area);

            // Roads
            var roadsCompiler = new RoadsCompiler(progress, projectDrive, assets.RoadTypeLibrary);

            roadsCompiler.Write(config, context.GetData<RoadsData>().Roads);
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Imagery
            var imageryCompiler = new ImageryCompiler(assets.Materials, progress, projectDrive);

            var tiles = imageryCompiler.Compile(config, CreateImagerySource(progress, config, context));
            progress.ReportOneDone();
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Objects + WRP
            var wrpBuilder = new WrpCompiler(progress, projectDrive);

            var grid = context.GetData<ElevationData>().Elevation;

            var size = area.SizeInMeters;

            var objects = GetObjects(progress, config, context, generators, grid)
                .Where(o => IsStrictlyInside(o, size));

            wrpBuilder.Write(config, grid, tiles, objects);
            progress.ReportOneDone();

            new DependencyUnpacker(assets, projectDrive).Unpack(progress, config, wrpBuilder);

            return new WrpAndContextResults(config, context, wrpBuilder.UsedModels, wrpBuilder.UsedRvmat);
        }

        protected virtual IImagerySource CreateImagerySource(IProgressTask progress, Arma3MapConfig config, IContext context)
        {
            return new ImagerySource(assets.Materials, progress, projectDrive, config, context);
        }

        protected virtual IEnumerable<EditableWrpObject> GetObjects(IProgressTask progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators, ElevationGrid grid)
        {
            return generators.Generators
                            .Progress(progress)
                            .SelectMany(tb => tb.Generate(config, context))
                            .Select(o => o.ToWrpObject(grid));
        }

        private bool IsStrictlyInside(EditableWrpObject o, float size)
        {
            return o.Transform.TranslateX >= 0 && o.Transform.TranslateX < size 
                && o.Transform.TranslateZ >= 0 && o.Transform.TranslateZ < size;
        }
    }
}
