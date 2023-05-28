using GameRealisticMap.Preview;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var progress = new ConsoleProgressSystem();

            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", 2.5f, 1024/*8*/);

            var render = new PreviewRender(area, new ImageryOptions());

            await render.RenderHtml(progress, Path.GetFullPath("preview.html"));

            //var x = JsonSerializer.Serialize(ImageTiler.DefaultToWebp(context.GetData<RawSatelliteImageData>().Image, "rawsat"));

            // new HillshaderFast(new MapToolkit.Vector(2.5, 2.5)).GetPixelsAlphaBelowFlat(context.GetData<ElevationData>().Elevation.ToDataCell()).SaveAsPng("elevation.png");

        }
    }
}