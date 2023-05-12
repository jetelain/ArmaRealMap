using BIS.PAA;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class FakeSatRender : IdMapRenderBase<Rgba32>
    {
        private readonly Dictionary<string, Image<Rgba32>?> cache = new Dictionary<string, Image<Rgba32>?>(StringComparer.OrdinalIgnoreCase);
        private readonly IGameFileSystem gameFileSystem;

        public FakeSatRender(TerrainMaterialLibrary materialLibrary, IProgressSystem progress, IGameFileSystem gameFileSystem)
            : base(materialLibrary, progress)
        {
            this.gameFileSystem = gameFileSystem;
        }

        public override Image<Rgba32> Render(IArma3MapConfig config, IContext context)
        {
            var image = base.Render(config, context);
            image.Mutate(d => d.GaussianBlur(10f));
            return image;
        }

        protected override IBrush GetBrush(TerrainMaterial material)
        {
            if (!cache.TryGetValue(material.ColorTexture, out var image))
            {
                cache.Add(material.ColorTexture, image = LoadImage(material));
            }
            if (image == null)
            {
                throw new Exception($"Texture '{material.ColorTexture}' was not found, unable to generate fake satmap.");
            }
            return new ImageBrush(image);
        }

        private Image<Rgba32>? LoadImage(TerrainMaterial material)
        {
            if (material.FakeSatPngImage != null)
            {
                return Image.Load<Rgba32>(material.FakeSatPngImage, new PngDecoder());
            }
            using var paaStream = gameFileSystem.OpenFileIfExists(material.ColorTexture);
            if (paaStream == null)
            {
                return LoadPngFallback(material);
            }
            var paa = new PAA(paaStream);
            var map = paa.Mipmaps.First(m => m.Width == 8);
            var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
            return Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height).CloneAs<Rgba32>();
        }

        private Image<Rgba32>? LoadPngFallback(TerrainMaterial material)
        {
            using var pngStream = gameFileSystem.OpenFileIfExists(Path.ChangeExtension(material.ColorTexture, ".png"));
            if (pngStream == null)
            {
                return null;
            }
            var image = Image.Load<Rgba32>(pngStream, new PngDecoder());
            image.Mutate(i => i.Resize(8, 8));
            return image;
        }

        protected override IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return config.TerrainToPixel(points);
        }
    }
}
