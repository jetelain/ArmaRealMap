using System.Numerics;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.Algorithms.Randomizations
{
    public class ScaleUniformRandomizationTest
    {
        [Fact]
        public void GetMatrix_MinEqualsMax_ProducesUniformScale()
        {
            var op = new ScaleUniformRandomization(2f, 2f, Vector3.Zero);
            var matrix = op.GetMatrix(new Random(0), Vector3.Zero).Round(3);

            Assert.Equal(new Matrix4x4(
                2, 0, 0, 0,
                0, 2, 0, 0,
                0, 0, 2, 0,
                0, 0, 0, 1), matrix);
        }

        [Fact]
        public void GetMatrix_ScaleOne_ProducesIdentity()
        {
            var op = new ScaleUniformRandomization(1f, 1f, Vector3.Zero);
            var matrix = op.GetMatrix(new Random(0), Vector3.Zero).Round(3);

            Assert.Equal(Matrix4x4.Identity, matrix);
        }
    }
}
