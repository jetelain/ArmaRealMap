using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public static class Arma3MapConfigHelper
    {
        public static Size GetImagerySize(this IArma3MapConfig config)
        {
            return new Size((int)Math.Ceiling(config.SizeInMeters / config.Resolution));
        }

        public static double GetTextureScale(this IArma3MapConfig config)
        {
            return config.SizeInMeters / WrpBuilder.LandRange / config.TextureSizeInMeters;
        }
    }
}
