using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Osm;
using GameRealisticMap.Preview;
using GameRealisticMap.Reporting;
using PdfSharpCore.Pdf.Filters;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using MapToolkit.DataCells;

namespace GameRealisticMap.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var progress = new ConsoleProgressSystem();

            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", 2.5f, 1024/*8*/);

            var render = new PreviewRender(area, new ImageryOptions());

            await render.RenderHtml(progress, Path.GetFullPath("preview.html"), true);

            //var x = JsonSerializer.Serialize(ImageTiler.DefaultToWebp(context.GetData<RawSatelliteImageData>().Image, "rawsat"));

            // new HillshaderFast(new MapToolkit.Vector(2.5, 2.5)).GetPixelsAlphaBelowFlat(context.GetData<ElevationData>().Elevation.ToDataCell()).SaveAsPng("elevation.png");

            //var catalog = new BuildersCatalog(progress, new DefaultRoadTypeLibrary(), true);
            //var loader = new OsmDataOverPassLoader(progress);
            //var osmSource = await loader.Load(area);
            //var context = new BuildContext(catalog, progress, area, osmSource, new ImageryOptions());
            //var elevation = context.GetData<ElevationData>();

            //elevation.Elevation.ToDataCell().SaveImagePreview(Path.GetFullPath("preview.png"));
        }

    }
}