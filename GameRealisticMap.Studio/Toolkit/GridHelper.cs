using System;
using System.Linq;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class GridHelper
    {
        internal static int[] Arma3GridSizes = [256, 512, 1024, 2048, 4096, 8192];
        internal static int[] GenericGridSizes = [256, 512, 1024, 2048, 4096, 8192, 16384]; 

        public static int GetGridSize(int[] gridSizes, float value)
        {
            foreach (var candidate in gridSizes)
            {
                var cellsize = value / candidate;
                if (cellsize > 2 && cellsize < 8)
                {
                    return candidate;
                }
            }
            return gridSizes.Max();
        }

        public static float NormalizeCellSize(float v)
        {            
            // 0.125 precision
            // Map sizes are enforced as multiple of 32 meters
            return MathF.Round(v * 8) / 8;
        }
    }
}
