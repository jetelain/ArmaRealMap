using GameRealisticMap;

namespace GameRealisticMap.Test
{
    public class MathHelperTest
    {
        [Fact]
        public void ToRadians_Zero_ReturnsZero()
        {
            Assert.Equal(0f, MathHelper.ToRadians(0f));
        }

        [Fact]
        public void ToRadians_180_ReturnsPi()
        {
            Assert.Equal(MathF.PI, MathHelper.ToRadians(180f), 5);
        }

        [Fact]
        public void ToRadians_90_ReturnsHalfPi()
        {
            Assert.Equal(MathF.PI / 2f, MathHelper.ToRadians(90f), 5);
        }

        [Fact]
        public void ToRadians_360_ReturnsTwoPi()
        {
            Assert.Equal(MathF.PI * 2f, MathHelper.ToRadians(360f), 5);
        }

        [Fact]
        public void FromRadians_Zero_ReturnsZero()
        {
            Assert.Equal(0f, MathHelper.FromRadians(0.0));
        }

        [Fact]
        public void FromRadians_Pi_Returns180()
        {
            Assert.Equal(180f, MathHelper.FromRadians(Math.PI), 4);
        }

        [Fact]
        public void FromRadians_HalfPi_Returns90()
        {
            Assert.Equal(90f, MathHelper.FromRadians(Math.PI / 2.0), 4);
        }

        [Fact]
        public void ToRadians_FromRadians_RoundTrip()
        {
            var angles = new float[] { 0f, 45f, 90f, 135f, 180f, 270f, 360f };
            foreach (var angle in angles)
            {
                Assert.Equal(angle, MathHelper.FromRadians(MathHelper.ToRadians(angle)), 3);
            }
        }
    }
}
