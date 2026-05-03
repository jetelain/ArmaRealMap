using System;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    /// <summary>
    /// Provides 2D preview images for Arma 3 P3D models and PAA textures.
    /// Fast previews return a placeholder URI immediately; <c>GetPreview</c> resolves
    /// to an actual rendered thumbnail asynchronously.
    /// </summary>
    internal interface IArma3Previews
    {
        Uri GetPreviewFast(string modelPath);

        Task<Uri> GetPreview(string modelPath);

        Uri? GetTexturePreview(string texture); 
        
        Uri? GetTexturePreviewSmall(string texture, int size = 512);
    }
}
