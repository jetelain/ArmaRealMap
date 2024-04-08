﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    public static class Arma3ColorRender
    {
        private static byte[] Red = new byte[] { 0, 1, 3, 4, 6, 8, 9, 11, 12, 14, 16, 17, 19, 21, 22, 24, 26, 27, 29, 31, 32, 34, 36, 38, 39, 41, 43, 45, 47, 49, 51, 53, 55, 56, 58, 60, 62, 65, 67, 69, 71, 73, 75, 77, 79, 81, 84, 86, 88, 90, 92, 94, 96, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119, 122, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 142, 144, 146, 148, 149, 151, 153, 154, 156, 158, 159, 161, 163, 164, 166, 167, 169, 170, 172, 173, 174, 176, 177, 179, 180, 181, 183, 184, 185, 186, 188, 189, 190, 191, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 212, 213, 214, 215, 215, 216, 217, 218, 218, 219, 220, 220, 221, 222, 222, 223, 223, 224, 225, 225, 226, 226, 227, 227, 228, 228, 229, 229, 229, 230, 230, 231, 231, 231, 232, 232, 233, 233, 233, 234, 234, 234, 235, 235, 235, 235, 236, 236, 236, 237, 237, 237, 237, 238, 238, 238, 238, 238, 239, 239, 239, 239, 240, 240, 240, 240, 240, 240, 241, 241, 241, 241, 241, 242, 242, 242, 242, 242, 242, 243, 243, 243, 243, 243, 243, 243, 244, 244, 244, 244, 244, 244, 244, 245, 245, 245, 245, 245, 245, 245, 246, 246, 246, 246, 246, 246, 246, 246, 246, 247, 247, 247, 247, 247, 247, 247, 247, 248, 248, 248, 248, 248, 248, 248, 248, 248, 249 };
        private static byte[] RedReverse = Reverse(Red);

        private static byte[] Green = new byte[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 13, 14, 15, 17, 18, 19, 20, 22, 23, 25, 26, 27, 29, 30, 32, 34, 35, 37, 38, 40, 42, 44, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71, 73, 75, 77, 80, 82, 84, 86, 88, 90, 93, 95, 97, 99, 101, 103, 105, 107, 110, 112, 114, 116, 118, 120, 121, 123, 125, 127, 129, 131, 133, 135, 136, 138, 140, 142, 143, 145, 147, 149, 150, 152, 153, 155, 157, 158, 160, 161, 163, 164, 166, 167, 169, 170, 171, 173, 174, 176, 177, 178, 179, 181, 182, 183, 184, 186, 187, 188, 189, 190, 191, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 204, 205, 206, 207, 208, 209, 209, 210, 211, 212, 212, 213, 214, 215, 215, 216, 217, 217, 218, 218, 219, 220, 220, 221, 221, 222, 222, 223, 223, 224, 224, 225, 225, 226, 226, 227, 227, 228, 228, 228, 229, 229, 229, 230, 230, 231, 231, 231, 232, 232, 232, 232, 233, 233, 233, 234, 234, 234, 234, 235, 235, 235, 236, 236, 236, 236, 236, 237, 237, 237, 237, 238, 238, 238, 238, 238, 239, 239, 239, 239, 239, 240, 240, 240, 240, 240, 241, 241, 241, 241, 241, 241, 242, 242, 242, 242, 242, 242, 243, 243, 243, 243, 243, 243, 243, 244, 244, 244, 244, 244, 244, 244, 245, 245, 245, 245, 245, 245, 245, 246, 246, 246, 246, 246, 246, 246, 247 };
        private static byte[] GreenReverse = Reverse(Green);

        private static byte[] Blue = new byte[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 12, 13, 14, 16, 17, 18, 19, 21, 22, 23, 25, 26, 28, 29, 30, 32, 33, 35, 36, 38, 39, 41, 43, 44, 46, 47, 49, 51, 53, 54, 56, 58, 60, 62, 63, 65, 67, 69, 71, 73, 75, 77, 78, 80, 82, 84, 86, 88, 90, 92, 94, 96, 98, 100, 102, 104, 105, 107, 109, 111, 113, 115, 117, 118, 120, 122, 124, 126, 128, 129, 131, 133, 135, 136, 138, 140, 141, 143, 145, 147, 148, 150, 151, 153, 155, 156, 158, 159, 161, 162, 164, 165, 167, 168, 170, 171, 173, 174, 176, 177, 178, 180, 181, 182, 184, 185, 186, 187, 188, 190, 191, 192, 193, 194, 195, 196, 197, 198, 200, 200, 201, 202, 203, 204, 205, 206, 207, 208, 208, 209, 210, 211, 212, 212, 213, 214, 214, 215, 216, 216, 217, 218, 218, 219, 219, 220, 220, 221, 221, 222, 222, 223, 223, 224, 224, 225, 225, 225, 226, 226, 227, 227, 227, 228, 228, 228, 229, 229, 229, 230, 230, 230, 231, 231, 231, 232, 232, 232, 232, 233, 233, 233, 234, 234, 234, 234, 235, 235, 235, 235, 236, 236, 236, 236, 237, 237, 237, 237, 238, 238, 238, 238, 239, 239, 239, 239, 240, 240, 240, 240, 241, 241, 241, 241, 242, 242, 242, 242, 242, 243, 243, 243, 243, 244, 244, 244, 244, 244, 245, 245, 245, 245, 246, 246, 246, 246, 247, 247, 247, 247, 247, 248, 248, 248, 248, 249  };
        private static byte[] BlueReverse = Reverse(Blue);

        private static byte[] Reverse(byte[] data)
        {
            var reverse = new byte[256];
            for(int i = 0; i<256; i++)
            {
                var idx = Array.IndexOf(data, i);
                if (idx == -1)
                {
                    idx = Array.IndexOf(data, data.OrderBy(v => Math.Abs(v - i)).First());
                    if (idx == -1)
                    {
                        idx = 255;
                    }
                }
                reverse[i] = (byte)idx;
            }
            return reverse;
        }

        /// <summary>
        /// Convert a texture color to the in-game render
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Bgra32 ToArma3(Bgra32 rgb)
        {
            return new Bgra32(Red[rgb.R], Green[rgb.G], Blue[rgb.B], rgb.A);
        }

        /// <summary>
        /// Convert a texture color to the in-game render
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Rgba32 ToArma3(Rgba32 rgb)
        {
            return new Rgba32(Red[rgb.R], Green[rgb.G], Blue[rgb.B], rgb.A);
        }

        /// <summary>
        /// Convert an in-game render color to a texture color
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Bgra32 FromArma3(Bgra32 rgb)
        {
            return new Bgra32(RedReverse[rgb.R], GreenReverse[rgb.G], BlueReverse[rgb.B], rgb.A);
        }

        /// <summary>
        /// Convert an in-game render color to a texture color
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public static Rgba32 FromArma3(Rgba32 rgb)
        {
            return new Rgba32(RedReverse[rgb.R], GreenReverse[rgb.G], BlueReverse[rgb.B], rgb.A);
        }

        public static void Mutate<TPixel>(Image<TPixel> img, Func<TPixel, TPixel> operation)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    img[x, y] = operation(img[x, y]);
                }
            }
        }
    }
}