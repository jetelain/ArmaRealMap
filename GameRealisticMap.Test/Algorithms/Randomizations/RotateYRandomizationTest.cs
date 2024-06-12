using System.Numerics;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.Algorithms.Randomizations
{
    public class RotateYRandomizationTest
    {
        [Fact]
        public void GetMatrix()
        {
            Assert.Equal(new Matrix4x4(
                 -1,  0,  0, 0, 
                  0,  1,  0, 0,
                  0,  0, -1, 0,
                  0,  0,  0, 1), new RotateYRandomization(180, 180, new Vector3(0, 0, 0)).GetMatrix(new Random(0), Vector3.Zero).Round(3));

            Assert.Equal(new Matrix4x4(
                 1, 0, 0, 0,
                 0, 1, 0, 0,
                 0, 0, 1, 0,
                 0, 0, 0, 1), new RotateYRandomization(0, 0, new Vector3(0, 0, 0)).GetMatrix(new Random(0), Vector3.Zero).Round(3));

        }
    }
}
