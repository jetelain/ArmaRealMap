using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal class GdtImporterItem
    {
        public GdtImporterItem(string colorTexture, string normalTexture, SurfaceConfig config)
        {
            ColorTexture = colorTexture;
            NormalTexture = normalTexture;
            Config = config;
        }

        public string ColorTexture { get; }
        public string NormalTexture { get; }
        public SurfaceConfig Config { get; }
    }
}
