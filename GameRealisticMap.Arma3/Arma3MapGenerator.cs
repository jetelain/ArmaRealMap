using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.Imagery;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.ManMade;
using GameRealisticMap.Arma3.Nature.Forests;
using GameRealisticMap.Arma3.Nature.Lakes;
using GameRealisticMap.Arma3.Nature.RockAreas;
using GameRealisticMap.Arma3.Nature.Scrubs;
using GameRealisticMap.Arma3.Nature.Trees;
using GameRealisticMap.Arma3.Nature.Watercourses;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Roads;
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

            // ManMade
            terrainBuilderLayerGenerators.Add(new BuildingGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new OrientedObjectsGenerator(progress, assets));
            // TODO: fences
            // TODO: railway

            // Nature
            terrainBuilderLayerGenerators.Add(new ForestEdgeGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new ForestGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new ForestRadialGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new LakeSurfaceGenerator(assets));
            terrainBuilderLayerGenerators.Add(new RocksGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new ScrubGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new ScrubRadialGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new TreesGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new WatercourseGenerator(progress, assets));
            terrainBuilderLayerGenerators.Add(new WatercourseRadialGenerator(progress, assets));
        }

        public void WriteDirectlyWrp(IArma3MapConfig config, IContext context)
        {
            // Roads
            var roadsCompiler = new RoadsCompiler(progress, gameFileSystemWriter, assets.RoadTypeLibrary);

            roadsCompiler.Write(config, context.GetData<RoadsData>().Roads);

            // Imagery
            var imageryCompiler = new ImageryCompiler(assets.Materials, progress, gameFileSystemWriter);

            var tiles = imageryCompiler.Compile(config, new ImagerySource(assets.Materials, progress, gameFileSystem, config, context));

            // Objects + WRP
            var wrpBuilder = new WrpCompiler(progress, gameFileSystemWriter);

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
