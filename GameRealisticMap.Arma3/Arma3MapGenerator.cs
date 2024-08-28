using System.Collections.Concurrent;
using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using HugeImages.Storage;
using Pmad.ProgressTracking;

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

        public async Task<IImagerySource?> GetImagerySource(IProgressScope progress, Arma3MapConfig a3config, IHugeImageStorage hugeImageStorage)
        {
            var context = await GetBuildContext(progress, a3config, hugeImageStorage); 
            if (context == null)
            {
                return null;
            }
            return new ImagerySource(assets.Materials, progress, projectDrive, a3config, context);
        }

        public async Task<IBuildContext?> GetBuildContext(IProgressScope progress, Arma3MapConfig a3config, IHugeImageStorage hugeImageStorage)
        {
            var osmSource = await LoadOsmData(progress, a3config);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            return CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);
        }

        protected virtual BuildContext CreateBuildContext(IProgressScope progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var builders = new BuildersCatalog(assets, sources);
            return new BuildContext(builders, progress, a3config.TerrainArea, osmSource, a3config.Imagery, hugeImageStorage);
        }

        [SupportedOSPlatform("windows")]
        public async Task<string?> GenerateMod(IProgressScope progress, Arma3MapConfig a3config)
        {
            var results = await GenerateWrp(progress, a3config);
            if (results == null || progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            Directory.CreateDirectory(a3config.TargetModDirectory);
            await pboCompilerFactory.Create(progress).BinarizeAndCreatePbo(a3config, results.UsedModels, results.UsedRvmat);

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

        public async Task<WrpAndContextResults?> GenerateWrp(IProgressScope progress, Arma3MapConfig a3config, bool pngToPaa = true)
        {
            var generators = new Arma3LayerGeneratorCatalog(assets);

            // Download from OSM
            var osmSource = await LoadOsmData(progress, a3config);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Generate content
            var context = CreateBuildContext(progress, a3config, osmSource);
            var results = await GenerateWrp(progress, a3config, context, a3config.TerrainArea, generators);
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

            return results;
        }

        protected virtual async Task<IOsmDataSource> LoadOsmData(IProgressScope progress, Arma3MapConfig a3config)
        {
            var loader = new OsmDataOverPassLoader(progress, sources);
            return await loader.Load(a3config.TerrainArea);
        }

        public async ValueTask<WrpAndContextResults?> GenerateWrp(IProgressScope progress, Arma3MapConfig config, IContext context, ITerrainArea area, Arma3LayerGeneratorCatalog generators)
        {
            // Game config
            new GameConfigGenerator(assets, projectDrive).Generate(config, context, area);

            // Roads
            var roadsCompiler = new RoadsCompiler(progress, projectDrive, assets.RoadTypeLibrary);

            roadsCompiler.Write(config, context.GetData<RoadsData>().Roads);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Imagery
            var imageryCompiler = new ImageryCompiler(assets.Materials, progress, projectDrive);

            var gridTask = context.GetDataAsync<ElevationData>();
            var tiles = await imageryCompiler.Compile(config, CreateImagerySource(progress, config, context));
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }

            // Objects + WRP
            var wrpBuilder = new WrpCompiler(progress, projectDrive);

            var grid = (await gridTask).Elevation;
            var size = area.SizeInMeters;

            var objects = (await GetObjects(progress, config, context, generators, grid))
                .Where(o => IsStrictlyInside(o, size));

            wrpBuilder.Write(config, grid, tiles, objects);

            new DependencyUnpacker(assets, projectDrive).Unpack(progress, config, wrpBuilder);

            return new WrpAndContextResults(config, context, wrpBuilder.UsedModels, wrpBuilder.UsedRvmat);
        }

        protected virtual IImagerySource CreateImagerySource(IProgressScope progress, Arma3MapConfig config, IContext context)
        {
            return new ImagerySource(assets.Materials, progress, projectDrive, config, context);
        }

        protected virtual async Task<IEnumerable<EditableWrpObject>> GetObjects(IProgressScope progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators, ElevationGrid grid)
        {
            return (await GenerateObjects(progress, config, context, generators))
                .SelectMany(o => o)
                .Select(o => o.ToWrpObject(grid));
        }

        private async ValueTask<IEnumerable<IEnumerable<TerrainBuilderObject>>> GenerateObjects(IProgressScope progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators)
        {
            var result = new ConcurrentQueue<IEnumerable<TerrainBuilderObject>>();
            using (var scope = progress.CreateScope("Objects", generators.Generators.Count))
            {
                await Parallel.ForEachAsync(generators.Generators, new ParallelOptions() { CancellationToken = progress.CancellationToken, MaxDegreeOfParallelism = 4 }, async (tb, _) =>
                {
                    result.Enqueue(await tb.Generate(config, context, scope));
                });
            }
            return result;
        }

        private bool IsStrictlyInside(EditableWrpObject o, float size)
        {
            return o.Transform.TranslateX >= 0 && o.Transform.TranslateX < size 
                && o.Transform.TranslateZ >= 0 && o.Transform.TranslateZ < size;
        }
    }
}
