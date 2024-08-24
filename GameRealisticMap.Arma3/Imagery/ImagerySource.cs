using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using HugeImages;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class ImagerySource : IImagerySource
    {
        private readonly TerrainMaterialLibrary materialLibrary;
        private readonly IProgressScope progress;
        private readonly SatMapRender satMapRender;
        private readonly IArma3MapConfig config;
        private readonly IContext context;

        public ImagerySource(TerrainMaterialLibrary materialLibrary, IProgressScope progress, IGameFileSystem gameFileSystem, IArma3MapConfig config, IContext context)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            satMapRender = new SatMapRender(materialLibrary, progress, gameFileSystem);
            this.config = config;
            this.context = context;
        }

        public async Task<HugeImage<Rgba32>> CreateIdMap()
        {
            using var step = progress.CreateScope("Draw IdMap");
            return await new IdMapRender(materialLibrary, step).Render(config, context);
        }

        public Task<Image> CreatePictureMap()
        {
            return satMapRender.RenderPictureMap(config, context, 2048);
        }

        public Task<HugeImage<Rgba32>> CreateSatMap()
        {
            return satMapRender.Render(config, context);
        }

        public Image CreateSatOut()
        {
            return satMapRender.RenderSatOut(config, context, config.TileSize / 2);
        }
    }
}
