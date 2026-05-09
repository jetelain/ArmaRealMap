using System.Numerics;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.Algorithms.Randomizations
{
    public class TranslateRadiusXYRandomizationTest
    {
        [Fact]
        public void GetMatrix_ZeroRadius_ProducesIdentity()
        {
            var matrix = new TranslateRadiusXYRandomization(0, 0).GetMatrix(new Random(0), Vector3.Zero).Round(3);

            Assert.Equal(new Matrix4x4(
                 1, 0, 0, 0,
                 0, 1, 0, 0,
                 0, 0, 1, 0,
                 0, 0, 0, 1), matrix);
        }

        [Fact]
        public void GetMatrix_RotationPartIsAlwaysIdentity()
        {
            var op = new TranslateRadiusXYRandomization(5, 10);
            var random = new Random(42);
            for (int i = 0; i < 50; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                Assert.Equal(1f, m.M11);
                Assert.Equal(0f, m.M12);
                Assert.Equal(0f, m.M13);
                Assert.Equal(0f, m.M21);
                Assert.Equal(1f, m.M22);
                Assert.Equal(0f, m.M23);
                Assert.Equal(0f, m.M31);
                Assert.Equal(0f, m.M32);
                Assert.Equal(1f, m.M33);
                Assert.Equal(1f, m.M44);
            }
        }

        [Fact]
        public void GetMatrix_ZTranslationIsAlwaysZero()
        {
            // XY plane: the Z component of the translation must always be zero
            var op = new TranslateRadiusXYRandomization(1, 10);
            var random = new Random(7);
            for (int i = 0; i < 100; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                Assert.Equal(0f, MathF.Round(m.M43, 5));
            }
        }

        [Fact]
        public void GetMatrix_TranslationMagnitudeIsWithinRadius()
        {
            const float min = 3f;
            const float max = 7f;
            var op = new TranslateRadiusXYRandomization(min, max);
            var random = new Random(123);
            for (int i = 0; i < 100; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                var length = MathF.Sqrt(m.M41 * m.M41 + m.M42 * m.M42);
                Assert.InRange(length, min - 0.001f, max + 0.001f);
            }
        }

        [Fact]
        public void GetMatrix_FixedRadius_MagnitudeEqualsRadius()
        {
            const float radius = 5f;
            var op = new TranslateRadiusXYRandomization(radius, radius);
            var random = new Random(0);
            for (int i = 0; i < 20; i++)
            {
                var m = op.GetMatrix(random, Vector3.Zero);
                var length = MathF.Sqrt(m.M41 * m.M41 + m.M42 * m.M42);
                Assert.Equal(radius, length, 4);
            }
        }
    }
}
