using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Demo;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Arma3
{
    public class Arma3DemoMapGenerator : Arma3MapGenerator
    {
        private readonly string name;
        private readonly IDemoNaming demoNaming;

        public Arma3DemoMapGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive, string name, IDemoNaming? demoNaming = null)
            : base(assets, projectDrive)
        {
            this.name = name;
            this.demoNaming = demoNaming ?? new DefaultDemoNaming();
        }

        protected override Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            return Task.FromResult<IOsmDataSource>(new NoneOsmDataSource());
        }

        protected override BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var context = base.CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);

            var buildingSizes = Enum.GetValues<BuildingTypeId>().ToDictionary(id => id, id => assets.GetBuildings(id).Select(b => b.Size).ToList());

            new DemoMapGenerator(assets.RoadTypeLibrary, demoNaming).CreateInto(context, name, buildingSizes);

            return context;
        }

        protected override IEnumerable<EditableWrpObject> GetObjects(IProgressTask progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators, ElevationGrid grid)
        {
            return base.GetObjects(progress, config, context, generators, grid)
                .Concat(GetDemoObjects(config, context, grid));
        }

        private IEnumerable<EditableWrpObject> GetDemoObjects(IArma3MapConfig config, IContext context, ElevationGrid grid)
        {
            var objects = new List<EditableWrpObject>();

            return objects;
        }

        private Arma3MapConfig CreateConfig()
        {
            return new Arma3MapConfig(new Arma3MapConfigJson()
            {
                WorldName = $"grm_demo_{name}",
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
