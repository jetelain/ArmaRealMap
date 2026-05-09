using System.Numerics;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.Algorithms.Randomizations
{
    public class TranslateYRandomizationTest
    {
        [Fact]
        public void GetMatrix_ZeroTranslation_ProducesIdentity()
        {
            Assert.Equal(new Matrix4x4(
                 1, 0, 0, 0,
                 0, 1, 0, 0,
                 0, 0, 1, 0,
                 0, 0, 0, 1), new TranslateYRandomization(0, 0).GetMatrix(new Random(0), Vector3.Zero).Round(3));
        }

        [Fact]
        public void GetMatrix_FixedValue_TranslatesOnlyY()
        {
            var matrix = new TranslateYRandomization(180, 180).GetMatrix(new Random(0), Vector3.Zero).Round(3);

            Assert.Equal(0f, matrix.M41);
            Assert.Equal(180f, matrix.M42);
            Assert.Equal(0f, matrix.M43);
            // Rotation part is identity
            Assert.Equal(1f, matrix.M11);
            Assert.Equal(1f, matrix.M22);
            Assert.Equal(1f, matrix.M33);
            Assert.Equal(1f, matrix.M44);
        }

        [Fact]
        public void GetMatrix_MinEqualsMax_IsDeterministic()
        {
            var op = new TranslateYRandomization(45, 45);
            var m1 = op.GetMatrix(new Random(0), Vector3.Zero);
            var m2 = op.GetMatrix(new Random(99), Vector3.Zero);

            Assert.Equal(m1.Round(5), m2.Round(5));
        }

        [Fact]
        public void GetMatrix_XAndZ_AreAlwaysZero()
        {
            var op = new TranslateYRandomization(-90, 90);
            var random = new Random(42);
            for (int i = 0; i < 50; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                Assert.Equal(0f, m.M41);
                Assert.Equal(0f, m.M43);
            }
        }
    }
}
