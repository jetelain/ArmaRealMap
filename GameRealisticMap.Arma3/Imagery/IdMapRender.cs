using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Imagery
{
    internal class IdMapRender : IdMapRenderBase<Rgb24>
    {
        public IdMapRender(ITerrainMaterialLibrary materialLibrary, IProgressSystem progress)
            : base(materialLibrary, progress)
        {
            drawingOptions.GraphicsOptions.Antialias = false;
        }

        protected override IBrush GetBrush(ITerrainMaterial material)
        {
            return new SolidBrush(new Color(material.Id));
        }
        protected override IEnumerable<PointF> TerrainToPixel(IArma3MapConfig config, IEnumerable<TerrainPoint> points)
        {
            return config.TerrainToPixel(points);
        }
    }
}
