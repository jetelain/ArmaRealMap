using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    internal interface IArma3Previews
    {
        Uri GetPreviewFast(string modelPath);

        Task<Uri> GetPreview(string modelPath);

        Uri? GetTexturePreview(string texture);
    }
}
