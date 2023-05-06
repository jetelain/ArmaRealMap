using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3
{
    internal class Arma3MapGenerator
    {
        private readonly IArma3RegionAssets assets;
        private readonly IProgressSystem progress;
        private readonly IGameFileSystem gameFileSystem;
        private readonly IGameFileSystemWriter gameFileSystemWriter;
        private readonly List<ITerrainBuilderLayerGenerator> terrainBuilderLayerGenerators = new List<ITerrainBuilderLayerGenerator>();

        public Arma3MapGenerator(
            IArma3RegionAssets assets, 
            IProgressSystem progress,
            IGameFileSystem gameFileSystem, 
            IGameFileSystemWriter gameFileSystemWriter)
        {
            this.assets = assets;
            this.progress = progress;
            this.gameFileSystem = gameFileSystem;
            this.gameFileSystemWriter = gameFileSystemWriter;

            // All generators
            terrainBuilderLayerGenerators.Add(new LakeSurfaceGenerator(assets));
        }

        public void WriteDirectlyWrp(IArma3MapConfig config, IContext context)
        {
            var imageryCompiler = new ImageryCompiler(assets.Materials, progress, gameFileSystemWriter);

            var tiles = imageryCompiler.Compile(config, new ImagerySource(assets.Materials, progress, gameFileSystem, config, context));

            var wrpBuilder = new WrpBuilder(progress, gameFileSystemWriter);

            var grid = context.GetData<ElevationData>().Elevation;

            wrpBuilder.Write(config, grid, tiles, GetObjects(config, context, grid));
        }

        private IEnumerable<EditableWrpObject> GetObjects(IArma3MapConfig config, IContext context, ElevationGrid elevationGrid)
        {
            return terrainBuilderLayerGenerators
                .SelectMany(tb => tb.Generate(config, context))
                .Select(o => o.ToWrpObject(elevationGrid));
        }
    }
}
