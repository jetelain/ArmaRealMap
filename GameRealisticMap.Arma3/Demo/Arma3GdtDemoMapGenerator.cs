using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Arma3.Demo
{
    public sealed class Arma3GdtDemoMapGenerator : Arma3MapGenerator
    {
        public Arma3GdtDemoMapGenerator(IEnumerable<TerrainMaterialDefinition> definitions, ProjectDrive projectDrive, IPboCompilerFactory pboCompilerFactory)
            : base(CreateAssets(definitions), projectDrive, pboCompilerFactory)
        {

        }
        private static IArma3RegionAssets CreateAssets(IEnumerable<TerrainMaterialDefinition> definitions)
        {
            var assets = new Arma3Assets() { Materials = new TerrainMaterialLibrary(definitions.ToList()) };
            assets.Roads.AddRange(Enum.GetValues<RoadTypeId>().Select(r => new Arma3RoadTypeInfos(r, new SixLabors.ImageSharp.PixelFormats.Rgb24(), 1, 
                "a3\\roads_f\\roads_ae\\data\\surf_roadtarmac_highway_ca.paa",
                "a3\\roads_f\\roads_ae\\data\\surf_roadtarmac_highway_end_ca.paa",
                "a3\\roads_f\\roads_ae\\data\\surf_roadtarmac_highway.rvmat", 1, 1)));
            return assets;
        }

        protected override Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            return Task.FromResult<IOsmDataSource>(new NoneOsmDataSource());
        }

        protected override BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var context = base.CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);

            var grid = new ElevationGrid(context.Area.GridSize, context.Area.GridCellSize);
            grid.Fill(15f);
            context.SetData(new RawElevationData(grid));

            var cities = new List<City>();

            context.SetData(new CitiesData(cities));
            return context;
        }

        protected override IEnumerable<EditableWrpObject> GetObjects(IProgressTask progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators, ElevationGrid grid)
        {
            return new List<EditableWrpObject>();
        }

        protected override IImagerySource CreateImagerySource(IProgressTask progress, Arma3MapConfig config, IContext context)
        {
            return new Arma3GdtDemoImagerySource(progress, config, context, assets.Materials);
        }

        private Arma3MapConfig CreateConfig()
        {
            return new Arma3MapConfig(new Arma3MapConfigJson()
            {
                WorldName = $"grm_demo_gdt_library",
                GridSize = 1024,
                GridCellSize = 5,
                FakeSatBlend = 1,
                SouthWest = "0, 0",
                Resolution = 1,
                TileSize = 512
            });
        }

        public async Task<Arma3MapConfig> GenerateWrp(IProgressTask progress)
        {
            var config = CreateConfig();
            await GenerateWrp(progress, config);
            return config;
        }

        [SupportedOSPlatform("windows")]
        public async Task<Arma3MapConfig> GenerateMod(IProgressTask progress)
        {
            var config = CreateConfig();
            await GenerateMod(progress, config);
            return config;
        }
    }
}
