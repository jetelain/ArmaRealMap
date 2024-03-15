using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class IdMapRender : IdMapRenderBase<Rgba32>
    {
        public IdMapRender(TerrainMaterialLibrary materialLibrary, IProgressSystem progress)
            : base(materialLibrary, progress)
        {
            drawingOptions.GraphicsOptions.Antialias = false;
        }

        protected override Brush GetBrush(TerrainMaterial material)
        {
            return new SolidBrush(new Color(material.Id));
        }

        protected override Brush GetEdgeBrush(TerrainMaterial material)
        {
            return new PatternBrush(new Color(material.Id), Color.Transparent, GeneratePattern(GetSeed(material)));
        }

        protected override IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return config.TerrainToPixel(points);
        }
    }
}
