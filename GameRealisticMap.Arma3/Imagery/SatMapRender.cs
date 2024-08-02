using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;
using GameRealisticMap.Satellite;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using HugeImages;
using HugeImages.Processing;
using HugeImages.Storage;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class SatMapRender
    {
        private readonly FakeSatRender fakeSatRender;
        private readonly IProgressScope progress;

        public SatMapRender(FakeSatRender fakeSatRender, IProgressScope progress)
        {
            this.fakeSatRender = fakeSatRender;
            this.progress = progress;
        }

        public SatMapRender(TerrainMaterialLibrary materialLibrary, IProgressScope progress, IGameFileSystem gameFileSystem)
            : this(new FakeSatRender(materialLibrary, progress, gameFileSystem), progress)
        {

        }

        public Image RenderSatOut(IArma3MapConfig config, IContext context, int size)
        {
            using var step = progress.CreateSingle("SatOut");
            return fakeSatRender.RenderSatOut(config, context, size);
        }

        public Image RenderPictureMap(IArma3MapConfig config, IContext context, int size)
        {
            HugeImage<Rgba32> satMap;

            if (config.FakeSatBlend == 1)
            {
                satMap = fakeSatRender.Render(config, context);
            }
            else
            {
                satMap = context.GetData<RawSatelliteImageData>().Image;
            }

            // TODO: Add shadows based on elevation data

            return satMap.ToScaledImageAsync(size, size).GetAwaiter().GetResult();
        }

        public HugeImage<Rgba32> Render(IArma3MapConfig config, IContext context)
        {
            var result = RenderBaseImageAsync(config, context).GetAwaiter().GetResult();

            // TODO: Add perlin noise ? (in natural areas ?)

            // TODO: Maybe also add shadows based on elevation data ?

            DrawRoads(config, context, result);

            return result;
        }

        private void DrawRoads(IArma3MapConfig config, IContext context, HugeImage<Rgba32> result)
        {
            var roads = context.GetData<RoadsData>().Roads;
            using var report = progress.CreateSingle("DrawRoads");
            result.MutateAllAsync(d =>
            {
                foreach (var road in roads.Where(r => r.SpecialSegment != WaySpecialSegment.Bridge))
                {
                    foreach (var polygon in road.Polygons)
                    {
                        PolygonDrawHelper.DrawPolygon(d, polygon, GetBrush((Arma3RoadTypeInfos)road.RoadTypeInfos), config.TerrainToSatMapPixel);
                    }
                }
            }).GetAwaiter().GetResult();
        }

        private async Task<HugeImage<Rgba32>> RenderBaseImageAsync(IArma3MapConfig config, IContext context)
        {
            if (config.FakeSatBlend == 1)
            {
                return fakeSatRender.Render(config, context);
            }

            var satMap = context.GetData<RawSatelliteImageData>().Image;

            if (config.UseColorCorrection)
            {
                using var report = progress.CreateSingle("SatColorCorrection");
                satMap = await satMap.CloneAsync(context.HugeImageStorage, "SatColorCorrection");
                await Parallel.ForEachAsync(satMap.PartsLoadedFirst, async (part, _) =>
                {
                    using (var token = await part.AcquireAsync().ConfigureAwait(false))
                    {
                        Arma3ColorRender.Mutate(token.GetImageReadWrite(), Arma3ColorRender.FromArma3);
                    }
                });
            }

            if (config.FakeSatBlend != 0)
            {
                using (var fakeSat = fakeSatRender.Render(config, context))
                {
                    using var report = progress.CreateSingle("FakeSatBlend");
                    await satMap.MutateAllAsync(async d =>
                    {
                         await d.DrawHugeImageAsync(fakeSat, Point.Empty, config.FakeSatBlend);
                    });
                }
            }
            return satMap;
        }

        private Brush GetBrush(Arma3RoadTypeInfos roadTypeInfos)
        {
            return new SolidBrush(roadTypeInfos.SatelliteColor);
        }
    }
}
