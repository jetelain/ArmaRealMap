using DatasetsLoader;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.Weather;
using GameRealisticMap.Satellite;
using Pmad.HugeImages;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace GameRealisticMap.Test
{

    public class DatasetBasedTests
    {
        private readonly ITestOutputHelper output;

        public DatasetBasedTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Chaux()
        {
            var context = await GetContext(Datasets.Chaux);

            AssertLandOnly(context);
        }

        [Fact]
        public async Task WindTurbine()
        {
            var context = await GetContext(Datasets.WindTurbine);

            AssertLandOnly(context);

            var buildings = context.GetData<BuildingsData>();
            var turbines = buildings.Buildings.Where(b => b.TypeId == BuildingTypeId.WindTurbine).ToList();
            Assert.NotEmpty(turbines);
        }

        [Fact]
        public async Task Northern()
        {
            var context = await GetContext(Datasets.Northern);

            AssertLandOnly(context);
        }

        [Fact]
        public async Task Island()
        {
            var context = await GetContext(Datasets.Island);

            var ocean = context.GetData<OceanData>();
            Assert.Single(ocean.Polygons);
            Assert.NotEmpty(ocean.Land);
            Assert.True(ocean.IsIsland);
            Assert.InRange(ocean.Polygons.Sum(p => p.Area), 60882200, 60882400);
            Assert.InRange(ocean.Land.Sum(p => p.Area), 24052300, 24052500);
        }

        [Fact]
        public async Task Island2()
        {
            var context = await GetContextOnly(Datasets.Island2); // Because it's too slow, query only required data

            var ocean = context.GetData<OceanData>();
            Assert.Single(ocean.Polygons);
            Assert.NotEmpty(ocean.Land);
            Assert.True(ocean.IsIsland);
            Assert.InRange(ocean.Polygons.Sum(p => p.Area), 160708200, 160708400);
            Assert.InRange(ocean.Land.Sum(p => p.Area), 16501000, 16501100);
        }

        [Fact]
        public async Task Coastline()
        {
            var context = await GetContext(Datasets.Coastline);

            var ocean = context.GetData<OceanData>();
            Assert.Single(ocean.Polygons);
            Assert.Single(ocean.Land);
            Assert.False(ocean.IsIsland);
            Assert.InRange(ocean.Polygons.Sum(p => p.Area), 349050, 349150);
            Assert.InRange(ocean.Land.Sum(p => p.Area), 343100, 343200);
        }

        [Fact]
        public async Task Coastline2()
        {
            var context = await GetContext(Datasets.Coastline2);

            var ocean = context.GetData<OceanData>();
            Assert.NotEmpty(ocean.Polygons);
            Assert.NotEmpty(ocean.Land);
            Assert.False(ocean.IsIsland);
            Assert.InRange(ocean.Polygons.Sum(p => p.Area), 354009900, 354010100);
            Assert.InRange(ocean.Land.Sum(p => p.Area), 65420300, 65420500);
        }

        [Fact]
        public async Task Coastline3()
        {
            var context = await GetContext(Datasets.Coastline3);

            var ocean = context.GetData<OceanData>();
            Assert.NotEmpty(ocean.Polygons);
            Assert.Single(ocean.Land);
            Assert.False(ocean.IsIsland);
            Assert.InRange(ocean.Polygons.Sum(p => p.Area), 6320000, 6320200);
            Assert.InRange(ocean.Land.Sum(p => p.Area), 9424900, 9425000);
        }

        [Fact]
        public async Task Vineyards()
        {
            var context = await GetContext(Datasets.Vineyards);

            AssertLandOnly(context);

            var vineyards = context.GetData<VineyardData>();
            Assert.NotEmpty(vineyards.Polygons);
        }

        private static void AssertLandOnly(BuildContext context)
        {
            var ocean = context.GetData<OceanData>();
            Assert.Empty(ocean.Polygons);
            Assert.Single(ocean.Land);
            Assert.False(ocean.IsIsland);
        }

        private async Task<BuildContext> GetContextOnly(DatasetMap ds)
        {
            var builders = new BuildersCatalog(new DefaultBuildersConfig(), DefaultSourceLocations.Instance);
            var osm = await DatasetsLoader.Datasets.GetOsmDataSource(ds);
            var context = new BuildContext(builders, new TestProgressSystem(output), ds.TerrainArea, osm, new MapProcessingOptions(), new MemoryHugeImageStorage());
            context.SetData(new RawSatelliteImageData(new HugeImage<Rgba32>(context.HugeImageStorage, new Size(256, 256))));
            context.SetData(new WeatherData(null));
            context.SetData(new ElevationContourData(new List<TerrainPath>()));
            return context;
        }

        private async Task<BuildContext> GetContext(DatasetMap ds)
        {
            var context = await GetContextOnly(ds);
            foreach (var data in context.Catalog.GetAll(context))
            {
            }
            return context;
        }
    }
}
