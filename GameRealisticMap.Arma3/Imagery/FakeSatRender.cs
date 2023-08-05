using BIS.PAA;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Reporting;
using HugeImages;
using HugeImages.Processing;
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

        public override HugeImage<Rgba32> Render(IArma3MapConfig config, IContext context)
        {
            var image = base.Render(config, context);
            image.MutateAllAsync(d => d.GaussianBlur(1.5f)).GetAwaiter().GetResult();
            return image;
        }

        public Image<Rgba32> RenderSatOut(IArma3MapConfig config, IContext context, int size)
        {
            var isIsland = context.GetData<OceanData>().IsIsland;
            var image = new Image<Rgba32>(size, size);
            image.Mutate(d =>
            {
                d.Fill(GetBrush(materialLibrary.GetMaterialByUsage(isIsland ? TerrainMaterialUsage.OceanGround : TerrainMaterialUsage.Default)));
                d.GaussianBlur(1.5f);
            });
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

        protected override IBrush GetEdgeBrush(TerrainMaterial material)
        {
            var pattern = GeneratePattern(GetSeed(material));
            var transparent = new Rgba32(0, 0, 0, 0);
            var image = new Image<Rgba32>(pattern.GetLength(0), pattern.GetLength(1));
            image.Mutate(d => d.Fill(GetBrush(material)));
            for(var x = 0; x < image.Width; ++x)
            {
                for(var y = 0; y < image.Height; ++y)
                {
                    if (!pattern[x, y])
                    {
                        image[x, y] = transparent;
                    }
                }
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
