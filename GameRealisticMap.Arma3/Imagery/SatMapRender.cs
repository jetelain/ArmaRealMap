﻿using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Satellite;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using Pmad.HugeImages;
using Pmad.HugeImages.Processing;
using Pmad.HugeImages.Storage;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class SatMapRender
    {
        private readonly TerrainMaterialLibrary materialLibrary;
        private readonly IProgressScope progress;
        private readonly IGameFileSystem gameFileSystem;

        public SatMapRender(TerrainMaterialLibrary materialLibrary, IProgressScope progress, IGameFileSystem gameFileSystem)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            this.gameFileSystem = gameFileSystem;
        }

        public Image RenderSatOut(IArma3MapConfig config, IContext context, int size)
        {
            using var step = progress.CreateSingle("SatOut");
            return new FakeSatRender(materialLibrary, progress, gameFileSystem).RenderSatOut(config, context, size);
        }

        public async Task<Image> RenderPictureMap(IArma3MapConfig config, IContext context, int size)
        {
            HugeImage<Rgba32> satMap;

            if (config.FakeSatBlend == 1)
            {
                using var step = progress.CreateScope("Draw PictureMap");
                satMap = await new FakeSatRender(materialLibrary, step, gameFileSystem).Render(config, context);
            }
            else
            {
                satMap = (await context.GetDataAsync<RawSatelliteImageData>()).Image;
            }

            // TODO: Add shadows based on elevation data

            return await satMap.ToScaledImageAsync(size, size);
        }

        public async Task<HugeImage<Rgba32>> Render(IArma3MapConfig config, IContext context)
        {
            var result = await RenderBaseImageAsync(config, context);

            // TODO: Add perlin noise ? (in natural areas ?)

            // TODO: Maybe also add shadows based on elevation data ?

            await DrawRoads(config, context, result);

            return result;
        }

        private async Task DrawRoads(IArma3MapConfig config, IContext context, HugeImage<Rgba32> result)
        {
            var roads = context.GetData<RoadsData>().Roads;
            using var report = progress.CreateSingle("DrawRoads");
            await result.MutateAllAsync(d =>
            {
                foreach (var road in roads.Where(r => r.SpecialSegment != WaySpecialSegment.Bridge))
                {
                    foreach (var polygon in road.Polygons)
                    {
                        PolygonDrawHelper.DrawPolygon(d, polygon, GetBrush((Arma3RoadTypeInfos)road.RoadTypeInfos), config.TerrainToSatMapPixel);
                    }
                }
            });
        }

        private async Task<HugeImage<Rgba32>> RenderBaseImageAsync(IArma3MapConfig config, IContext context)
        {
            if (config.FakeSatBlend == 1)
            {
                using var scope = progress.CreateScope("Draw FakeSat");
                return await new FakeSatRender(materialLibrary, scope, gameFileSystem).Render(config, context);
            }

            var satMap = (await context.GetDataAsync<RawSatelliteImageData>()).Image;

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
                using var scope = progress.CreateScope("Draw FakeSat");

                using (var fakeSat = await new FakeSatRender(materialLibrary, scope, gameFileSystem).Render(config, context))
                {
                    using var report = scope.CreateSingle("FakeSatBlend");
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
