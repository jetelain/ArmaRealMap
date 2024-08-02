using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Nature.Weather;
using GameRealisticMap.Osm;
using GameRealisticMap.Satellite;
using HugeImages;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace GameRealisticMap.Test
{
    public class BuildersCatalogTest
    {
        private readonly ITestOutputHelper output;

        public BuildersCatalogTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Check that all builders can fetch required data, and does not generates error if no data is available
        /// </summary>
        [Fact]
        public void EmptyDataDoesNotGenerateAnyError()
        {
            var progress = new TestProgressSystem(output);
            var builders = new BuildersCatalog(new DefaultBuildersConfig(), new DefaultSourceLocations());
            var context = new BuildContext(builders, progress, TerrainAreaUTM.CreateFromCenter("0, 0", 1f, 256), new NoneOsmDataSource(), new MapProcessingOptions(), new MemoryHugeImageStorage());
            
            // Mock external sources
            context.SetData(new RawElevationData(new ElevationGrid(context.Area.GridSize, context.Area.GridCellSize)));
            context.SetData(new RawSatelliteImageData(new HugeImage<Rgba32>(context.HugeImageStorage, new Size(256,256)))); 
            context.SetData(new WeatherData(null));

            foreach(var data in builders.GetAll(context))
            {
                output.WriteLine("=> " + data.GetType().Name);
            }
        }
    }
}
