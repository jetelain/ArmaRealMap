using DatasetsLoader;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.Weather;
using GameRealisticMap.Satellite;
using HugeImages;
using HugeImages.Storage;
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
        }

        [Fact]
        public async Task Coastline()
        {
            var context = await GetContext(Datasets.Coastline);

            var ocean = context.GetData<OceanData>();
            Assert.Single(ocean.Polygons);
            Assert.Single(ocean.Land);
            Assert.False(ocean.IsIsland);
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

        private async Task<BuildContext> GetContext(DatasetMap ds)
        {
            var osm = await DatasetsLoader.Datasets.GetOsmDataSource(ds);
            var progress = new TestProgressSystem(output);
            var builders = new BuildersCatalog(new DefaultBuildersConfig(), new DefaultSourceLocations());
            var context = new BuildContext(builders, progress, ds.TerrainArea, osm, new MapProcessingOptions(), new MemoryHugeImageStorage());
            context.SetData(new RawSatelliteImageData(new HugeImage<Rgba32>(context.HugeImageStorage, new Size(256, 256))));
            context.SetData(new WeatherData(null));
            context.SetData(new ElevationContourData(new List<TerrainPath>()));
            foreach (var data in builders.GetAll(context))
            {
            }
            return context;
        }
    }
}
