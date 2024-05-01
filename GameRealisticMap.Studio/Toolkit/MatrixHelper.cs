using System.Numerics;
using System.Windows.Media;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class MatrixHelper
    {
        public static Matrix ToAerialWpfMatrix(this Matrix4x4 matrix)
        {
            return new Matrix(matrix.M11, matrix.M13, matrix.M31, matrix.M33, matrix.M41, matrix.M43);
        }

        public static MatrixTransform ToAerialWpfMatrixTransform(this Matrix4x4 matrix)
        {
            return new MatrixTransform(ToAerialWpfMatrix(matrix));
        }
    }
}
