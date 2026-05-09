using System.Numerics;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.Algorithms.Randomizations
{
    public class TranslateZRandomizationTest
    {
        [Fact]
        public void GetMatrix_ZeroTranslation_ProducesIdentity()
        {
            Assert.Equal(new Matrix4x4(
                 1, 0, 0, 0,
                 0, 1, 0, 0,
                 0, 0, 1, 0,
                 0, 0, 0, 1), new TranslateZRandomization(0, 0).GetMatrix(new Random(0), Vector3.Zero).Round(3));
        }

        [Fact]
        public void GetMatrix_FixedValue_TranslatesOnlyZ()
        {
            var matrix = new TranslateZRandomization(180, 180).GetMatrix(new Random(0), Vector3.Zero).Round(3);

            Assert.Equal(0f, matrix.M41);
            Assert.Equal(0f, matrix.M42);
            Assert.Equal(180f, matrix.M43);
            Assert.Equal(1f, matrix.M11);
            Assert.Equal(1f, matrix.M22);
            Assert.Equal(1f, matrix.M33);
            Assert.Equal(1f, matrix.M44);
        }

        [Fact]
        public void GetMatrix_MinEqualsMax_IsDeterministic()
        {
            var op = new TranslateZRandomization(45, 45);
            var m1 = op.GetMatrix(new Random(0), Vector3.Zero);
            var m2 = op.GetMatrix(new Random(99), Vector3.Zero);

            Assert.Equal(m1.Round(5), m2.Round(5));
        }

        [Fact]
        public void GetMatrix_XAndY_AreAlwaysZero()
        {
            var op = new TranslateZRandomization(-90, 90);
            var random = new Random(42);
            for (int i = 0; i < 50; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                Assert.Equal(0f, m.M41);
                Assert.Equal(0f, m.M42);
            }
        }
    }
}
