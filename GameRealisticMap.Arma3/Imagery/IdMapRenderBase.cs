using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
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
        protected readonly IProgressSystem progress;
        protected readonly DrawingOptions drawingOptions;

        public IdMapRenderBase(TerrainMaterialLibrary materialLibrary, IProgressSystem progress)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            drawingOptions = new DrawingOptions();
        }

        public virtual Image<TPixel> Render(IArma3MapConfig config, IContext context)
        {
            var size = config.GetImagerySize();

            var image = new Image<TPixel>(size.Width, size.Height);

            image.Mutate(d => d.Fill(GetBrush(materialLibrary.GetMaterialByUsage(TerrainMaterialUsage.Default))));

            var categories = context.GetData<CategoryAreaData>();

            DrawPolygons(config, image, TerrainMaterialUsage.DefaultUrban,
                categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Residential).SelectMany(c => c.PolyList));

            DrawPolygons(config, image, TerrainMaterialUsage.DefaultIndustrial,
                categories.Areas.Where(c => c.BuildingType == BuildingTypeId.Industrial).SelectMany(c => c.PolyList));

            DrawPolygons(config, image, TerrainMaterialUsage.ForestGround, context.GetData<ForestData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.Meadow, context.GetData<MeadowsData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.FarmLand, context.GetData<FarmlandsData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.Sand, context.GetData<SandSurfacesData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.Grass, context.GetData<GrassData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.RiverGround, context.GetData<WatercoursesData>().Polygons);

            DrawPolygons(config, image, TerrainMaterialUsage.LakeGround, context.GetData<ElevationWithLakesData>().Lakes.Select(l => l.TerrainPolygon));

            DrawPolygons(config, image, TerrainMaterialUsage.RockGround, context.GetData<RocksData>().Polygons);

            return image;
        }

        private void DrawPolygons(IArma3MapConfig config, Image<TPixel> image, TerrainMaterialUsage material, IEnumerable<TerrainPolygon> polygons)
        {
            DrawPolygons(config, image, GetBrush(materialLibrary.GetMaterialByUsage(material)), polygons);
        }

        protected abstract IBrush GetBrush(TerrainMaterial material);

        private void DrawPolygons(IArma3MapConfig config, Image<TPixel> image, IBrush brush, IEnumerable<TerrainPolygon> polygons)
        {
            image.Mutate(d =>
            {
                foreach (var polygon in polygons)
                {
                    PolygonDrawHelper.DrawPolygon(d, polygon, brush, drawingOptions, points => TerrainToPixel(config, points));
                }
            });
        }

        protected abstract IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points);
    }
}
