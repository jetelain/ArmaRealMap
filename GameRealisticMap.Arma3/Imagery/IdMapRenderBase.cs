﻿using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Surfaces;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
using Pmad.HugeImages;
using Pmad.HugeImages.Processing;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Imagery
{
    internal abstract class IdMapRenderBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        protected readonly TerrainMaterialLibrary materialLibrary;
        protected readonly IProgressScope scope;
        protected readonly DrawingOptions drawingOptions;

        public IdMapRenderBase(TerrainMaterialLibrary materialLibrary, IProgressScope progress)
        {
            this.materialLibrary = materialLibrary;
            this.scope = progress;
            drawingOptions = new DrawingOptions();
        }

        public virtual async Task<HugeImage<TPixel>> Render(IArma3MapConfig config, IContext context)
        {
            var size = GetImageSize(config);

            var image = new HugeImage<TPixel>(context.HugeImageStorage, GetType().Name, new Size(size.Width, size.Height));

            await image.MutateAllAsync(d =>
            {
                d.Fill(GetBrush(materialLibrary.GetMaterialByUsage(TerrainMaterialUsage.Default)));

                var categories = context.GetData<CategoryAreaData>();

                DrawPolygons(config, d, TerrainMaterialUsage.DefaultUrban,
                    categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Residential).SelectMany(c => c.PolyList));

                DrawPolygons(config, d, TerrainMaterialUsage.DefaultIndustrial,
                    categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Industrial).SelectMany(c => c.PolyList));

                DrawPolygons(config, d, TerrainMaterialUsage.OceanGround, context.GetData<OceanData>().Polygons);

                DrawPolygonsWithCrown(config, d, TerrainMaterialUsage.Coastline, 2.5f, context.GetData<CoastlineData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Grass, context.GetData<OrchardData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Grass, context.GetData<VineyardData>().Polygons);

                DrawPolygonsWithCrown(config, d, TerrainMaterialUsage.ForestGround, 2.5f, context.GetData<ForestData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Meadow, context.GetData<MeadowsData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.FarmLand, context.GetData<FarmlandsData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.IceSurface, context.GetData<IceSurfaceData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Sand, context.GetData<SandSurfacesData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Grass, context.GetData<GrassData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.RiverGround, context.GetData<WatercoursesData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.LakeGround, context.GetData<ElevationWithLakesData>().Lakes.Select(l => l.TerrainPolygon));

                DrawPolygons(config, d, TerrainMaterialUsage.ScreeSurface, context.GetData<ScreeData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.RockGround, context.GetData<RocksData>().Polygons);

                DrawPolygons(config, d, TerrainMaterialUsage.Asphalt, context.GetData<AsphaltData>().Polygons);

            });

            return image;
        }

        protected abstract Size GetImageSize(IArma3MapConfig config);

        private void DrawPolygons(IArma3MapConfig config, IImageProcessingContext d, TerrainMaterialUsage material, IEnumerable<TerrainPolygon> polygons)
        {
            DrawPolygons(config, d, GetBrush(materialLibrary.GetMaterialByUsage(material)), polygons);
        }

        private void DrawPolygonsWithCrown(IArma3MapConfig config, IImageProcessingContext d, TerrainMaterialUsage material, float crownSize, IEnumerable<TerrainPolygon> polygons)
        {
            DrawPolygons(config, d, GetBrush(materialLibrary.GetMaterialByUsage(material)), polygons);

            DrawPolygonsCrown(config, d, GetEdgeBrush(materialLibrary.GetMaterialByUsage(material)), polygons, crownSize);
        }

        protected abstract Brush GetBrush(TerrainMaterial material);

        protected abstract Brush GetEdgeBrush(TerrainMaterial material);

        private void DrawPolygons(IArma3MapConfig config, IImageProcessingContext d, Brush brush, IEnumerable<TerrainPolygon> polygons)
        {
            foreach (var polygon in polygons.WithProgress(scope, "DrawPolygons"))
            {
                DrawPolygon(config, brush, d, polygon);
            }
        }

        private void DrawPolygonsCrown(IArma3MapConfig config, IImageProcessingContext d, Brush crownBrush, IEnumerable<TerrainPolygon> polygons, float crownSize)
        {
            foreach (var polygon in polygons.WithProgress(scope, "DrawPolygonsCrown"))
            {
                foreach (var x in polygon.OuterCrown(crownSize))
                {
                    DrawPolygon(config, crownBrush, d, x);
                }
            }
        }

        private void DrawPolygon(IArma3MapConfig config, Brush brush, IImageProcessingContext d, TerrainPolygon polygon)
        {
            PolygonDrawHelper.DrawPolygon(d, polygon, brush, drawingOptions, points => TerrainToPixel(config, points));
        }

        protected abstract IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points);

        protected static int GetSeed(TerrainMaterial material)
        {
            return material.Id.R >> 16 | material.Id.G >> 8 | material.Id.B;
        }

        protected static bool[,] GeneratePattern(int seed, float coef = 0.5f, int size = 64)
        {
            var rnd = new Random(seed);
            var matrix = new bool[size, size];
            for (var x = 0; x < size; ++x)
            {
                for (var y = 0; y < size; ++y)
                {
                    matrix[x, y] = rnd.NextDouble() >= coef;
                }
            }
            return matrix;
        }

    }
}
