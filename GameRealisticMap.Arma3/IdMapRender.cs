using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3
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
    }
}
