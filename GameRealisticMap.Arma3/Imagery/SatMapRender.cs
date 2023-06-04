using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;
using GameRealisticMap.Satellite;
using HugeImages;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class SatMapRender
    {
        private readonly FakeSatRender fakeSatRender;

        public SatMapRender(FakeSatRender fakeSatRender)
        {
            this.fakeSatRender = fakeSatRender;
        }

        public SatMapRender(TerrainMaterialLibrary materialLibrary, IProgressSystem progress, IGameFileSystem gameFileSystem)
            : this(new FakeSatRender(materialLibrary, progress, gameFileSystem))
        {

        }

        public Image RenderSatOut(IArma3MapConfig config, IContext context, int size)
        {
            return fakeSatRender.RenderSatOut(config, context, size);
        }

        public Image RenderPictureMap(IArma3MapConfig config, IContext context, int size)
        {
            var satMap = context.GetData<RawSatelliteImageData>().Image;

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

            result.MutateAllAsync(d =>
            {
                foreach (var road in roads.Where(r => r.SpecialSegment != WaySpecialSegment.Bridge))
                {
                    foreach (var polygon in road.Polygons)
                    {
                        PolygonDrawHelper.DrawPolygon(d, polygon, GetBrush((Arma3RoadTypeInfos)road.RoadTypeInfos), config.TerrainToPixel);
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
            if (config.FakeSatBlend != 0)
            {
                using (var fakeSat = fakeSatRender.Render(config, context))
                {
                    await satMap.MutateAllAsync(async d =>
                    {
                         await d.DrawHugeImageAsync(fakeSat, Point.Empty, config.FakeSatBlend);
                    });
                }
            }
            return satMap;
        }

        private IBrush GetBrush(Arma3RoadTypeInfos roadTypeInfos)
        {
            return new SolidBrush(roadTypeInfos.SatelliteColor);
        }
    }
}
