using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Reporting;
using HugeImages;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Demo
{
    internal class Arma3GdtDemoImagerySource : IImagerySource
    {
        private readonly IProgressTask progress;
        private readonly Arma3MapConfig config;
        private readonly IContext context;
        private readonly TerrainMaterialLibrary materials;
        private readonly TerrainMaterialDefinition defaultMaterial;

        private const int SquareSize = 256;

        public Arma3GdtDemoImagerySource(IProgressTask progress, Arma3MapConfig config, IContext context, TerrainMaterialLibrary materials)
        {
            this.progress = progress;
            this.config = config;
            this.context = context;
            this.materials = materials;
            this.defaultMaterial = materials.Definitions[0];
        }

        public HugeImage<Rgba32> CreateIdMap()
        {
            Rgba32 defaultColor = default;
            defaultMaterial.Material.Id.ToRgba32(ref defaultColor);
            var size = config.GetImagerySize();
            var image = new HugeImage<Rgba32>(context.HugeImageStorage, GetType().Name, new Size(size.Width, size.Height), defaultColor);
            var x = 0;
            var y = 0;
            var opt = new DrawingOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = false } };
            image.MutateAllAsync(p =>
            {
                p.Fill(defaultColor);
                foreach (var def in materials.Definitions)
                {
                    p.Fill(opt, def.Material.Id, new RectangleF(x, y, SquareSize, SquareSize));
                    x += SquareSize;
                    if (x >= size.Width)
                    {
                        x = 0;
                        y += SquareSize;
                    }
                }
            }).GetAwaiter().GetResult();
            return image;
        }

        public Image CreatePictureMap()
        {
            var img = new Image<Rgba32>(256, 256, new Rgba32(255,255,255,255));
            return img;
        }

        public HugeImage<Rgba32> CreateSatMap()
        {
            var size = config.GetImagerySize();
            var image = new HugeImage<Rgba32>(context.HugeImageStorage, GetType().Name, new Size(size.Width, size.Height));
            var x = 0;
            var y = 0;
            var defaultBrush = new ImageBrush(Image.Load(defaultMaterial.Material.FakeSatPngImage));
            image.MutateAllAsync(p =>
            {
                p.Fill(defaultBrush);
                foreach (var def in materials.Definitions)
                {
                    var img = Image.Load(def.Material.FakeSatPngImage);
                    p.Fill(new ImageBrush(img), new RectangleF(x, y, SquareSize, SquareSize));
                    x += SquareSize;
                    if (x >= size.Width)
                    {
                        x = 0;
                        y += SquareSize;
                    }
                }
            }).GetAwaiter().GetResult();
            return image;

        }

        public Image CreateSatOut()
        {
            var img = new Image<Rgba32>(config.TileSize / 2, config.TileSize / 2);
            img.Mutate(p => p.Fill(new ImageBrush(Image.Load(defaultMaterial.Material.FakeSatPngImage))));
            return img;
        }
    }
}