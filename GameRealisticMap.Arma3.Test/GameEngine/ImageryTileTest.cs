using GameRealisticMap.Arma3.GameEngine;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class ImageryTileTest
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Act
            var tile = new ImageryTile(1, 2, 256, 128, 512, 1024, 0.5);

            // Assert
            Assert.Equal(1, tile.X);
            Assert.Equal(2, tile.Y);
            Assert.Equal(new Point(128,384), tile.ImageTopLeft);
            Assert.Equal(new Point(640,896), tile.ImageBottomRight);
            Assert.Equal(512, tile.Size);
            Assert.Equal(-0.25, tile.UB);
            Assert.Equal(1.0, tile.VB);
            Assert.Equal(0.5, tile.UA);
            Assert.Equal(new Point(256,512), tile.ContentTopLeft);
            Assert.Equal(new Point(512,768), tile.ContentBottomRight);
        }

        [Theory]
        [InlineData(400, 600, true)]
        [InlineData(400, 400, false)]
        [InlineData(400, 800, false)]
        [InlineData(200, 600, false)]
        [InlineData(600, 600, false)]
        public void ContainsImagePoint_ShouldReturnCorrectly(int px, int py, bool expected)
        {
            // Arrange
            var tile = new ImageryTile(1, 2, 256, 128, 512, 1024, 0.5);
            var point = new Point(px, py);

            // Act
            var result = tile.ContainsImagePoint(point);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
