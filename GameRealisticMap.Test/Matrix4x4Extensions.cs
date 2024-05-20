using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Test
{
    internal static class Matrix4x4Extensions
    {

        public static Matrix4x4 Round(this Matrix4x4 matrix, int digits)
        {
            return new Matrix4x4(
                MathF.Round(matrix.M11, digits),
                MathF.Round(matrix.M12, digits),
                MathF.Round(matrix.M13, digits),
                MathF.Round(matrix.M14, digits),

                MathF.Round(matrix.M21, digits),
                MathF.Round(matrix.M22, digits),
                MathF.Round(matrix.M23, digits),
                MathF.Round(matrix.M24, digits),

                MathF.Round(matrix.M31, digits),
                MathF.Round(matrix.M32, digits),
                MathF.Round(matrix.M33, digits),
                MathF.Round(matrix.M34, digits),

                MathF.Round(matrix.M41, digits),
                MathF.Round(matrix.M42, digits),
                MathF.Round(matrix.M43, digits),
                MathF.Round(matrix.M44, digits)
                );
        }

    }
}
