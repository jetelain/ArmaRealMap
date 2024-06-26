﻿using System;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    internal interface IArma3Previews
    {
        Uri GetPreviewFast(string modelPath);

        Task<Uri> GetPreview(string modelPath);

        Uri? GetTexturePreview(string texture); 
        
        Uri? GetTexturePreviewSmall(string texture, int size = 512);
    }
}
