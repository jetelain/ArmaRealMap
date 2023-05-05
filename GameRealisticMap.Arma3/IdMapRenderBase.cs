using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3
{
    internal abstract class IdMapRenderBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ITerrainMaterialLibrary materialLibrary;
        private readonly IProgressSystem progress;
        protected readonly DrawingOptions drawingOptions;

        public IdMapRenderBase(ITerrainMaterialLibrary materialLibrary, IProgressSystem progress)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            this.drawingOptions = new DrawingOptions();
        }

        public Image<TPixel> Generate(IArma3MapConfig config, IContext context)
        {
            var size = config.GetImagerySize();

            var image = new Image<TPixel>(size.Width, size.Height);

            image.Mutate(d => d.Fill(GetBrush(materialLibrary.Default)));

            var categories = context.GetData<CategoryAreaData>();

            DrawPolygons(config, image, materialLibrary.DefaultUrban, 
                categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Residential).SelectMany(c => c.PolyList));

            DrawPolygons(config, image, materialLibrary.DefaultIndustrial,
                categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Industrial).SelectMany(c => c.PolyList));

            DrawPolygons(config, image, materialLibrary.ForestGround, context.GetData<ForestData>().Polygons);

            DrawPolygons(config, image, materialLibrary.Meadow, context.GetData<MeadowsData>().Polygons);

            DrawPolygons(config, image, materialLibrary.FarmLand, context.GetData<FarmlandsData>().Polygons);

            DrawPolygons(config, image, materialLibrary.Sand, context.GetData<SandSurfacesData>().Polygons);

            DrawPolygons(config, image, materialLibrary.Grass, context.GetData<GrassData>().Polygons);

            DrawPolygons(config, image, materialLibrary.RiverGround, context.GetData<WatercoursesData>().Polygons);

            DrawPolygons(config, image, materialLibrary.LakeGround, context.GetData<ElevationWithLakesData>().Lakes.Select(l => l.TerrainPolygon));

            return image;
        }

        private void DrawPolygons(IArma3MapConfig config, Image<TPixel> image, ITerrainMaterial material, IEnumerable<TerrainPolygon> polygons)
        {
            DrawPolygons(config, image, GetBrush(material), polygons);
        }

        protected abstract IBrush GetBrush(ITerrainMaterial material);

        private void DrawPolygons(IArma3MapConfig config, Image<TPixel> image, IBrush brush, IEnumerable<TerrainPolygon> polygons)
        {
            image.Mutate(d =>
            {
                foreach(var polygon in polygons)
                {
                    PolygonDrawHelper.DrawPolygon(d, polygon, brush, points => TerrainToPixel(config, points));
                }
            });
        }

        protected virtual IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return points.Select(point => new PointF((float)(point.X / config.Resolution), (float)(point.Y / config.Resolution)));
        }
    }
}
