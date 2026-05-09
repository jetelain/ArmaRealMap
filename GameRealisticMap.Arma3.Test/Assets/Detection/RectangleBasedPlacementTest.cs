using System.Numerics;
using GameRealisticMap.Arma3.Assets.Detection;

namespace GameRealisticMap.Arma3.Test.Assets.Detection
{
    public class RectangleBasedPlacementTest
    {
        [Fact]
        public void Min_Max_AreComputedFromCenterAndSize()
        {
            var placement = new RectangleBasedPlacement(new Vector2(10, 20), new Vector2(4, 6));

            Assert.Equal(new Vector2(8, 17), placement.Min);
            Assert.Equal(new Vector2(12, 23), placement.Max);
        }

        [Fact]
        public void Surface_IsWidthTimesHeight()
        {
            var placement = new RectangleBasedPlacement(new Vector2(0, 0), new Vector2(3, 5));

            Assert.Equal(15f, placement.Surface);
        }

        [Fact]
        public void Center_And_Size_ArePreserved()
        {
            var center = new Vector2(7, 8);
            var size = new Vector2(2, 4);
            var placement = new RectangleBasedPlacement(center, size);

            Assert.Equal(center, placement.Center);
            Assert.Equal(size, placement.Size);
        }
    }
}
