using System.Numerics;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test
{
    public class Arma3MapConfigHelperTest
    {
        [Fact]
        public void GetSatMapSize()
        {
            Assert.Equal(new Size(1024, 1024), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1 }.GetSatMapSize());
            Assert.Equal(new Size(1024, 1024), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1, IdMapMultiplier = 2 }.GetSatMapSize());

            Assert.Equal(new Size(512, 512), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2 }.GetSatMapSize());
            Assert.Equal(new Size(512, 512), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2, IdMapMultiplier = 2 }.GetSatMapSize());
        }

        [Fact]
        public void GetIdMapSize()
        {
            Assert.Equal(new Size(1024, 1024), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1 }.GetIdMapSize());
            Assert.Equal(new Size(2048, 2048), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1, IdMapMultiplier = 2 }.GetIdMapSize());

            Assert.Equal(new Size(512, 512), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2 }.GetIdMapSize());
            Assert.Equal(new Size(1024, 1024), new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2, IdMapMultiplier = 2 }.GetIdMapSize());
        }

        [Fact]
        public void TerrainToSatMapPixel()
        {
            Assert.Equal([new(128, 896), new(256, 896), new(128, 768)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1 }.TerrainToSatMapPixel([new(128, 128), new(256, 128), new(128, 256)]));
            Assert.Equal([new(128, 896), new(256, 896), new(128, 768)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1, IdMapMultiplier = 2 }.TerrainToSatMapPixel([new(128, 128), new(256, 128), new(128, 256)]));

            Assert.Equal([new(64, 448), new(128, 448), new(64, 384)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2 }.TerrainToSatMapPixel([new(128, 128), new(256, 128), new(128, 256)]));
            Assert.Equal([new(64, 448), new(128, 448), new(64, 384)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2, IdMapMultiplier = 2 }.TerrainToSatMapPixel([new(128, 128), new(256, 128), new(128, 256)]));
        }

        [Fact]
        public void TerrainToIdMapPixel()
        {
            Assert.Equal([new(128, 896), new(256, 896), new(128, 768)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1 }.TerrainToIdMapPixel([new (128,128), new(256,128), new(128, 256)]));
            Assert.Equal([new(256, 1792), new(512, 1792), new(256, 1536)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 1, IdMapMultiplier = 2 }.TerrainToIdMapPixel([new(128, 128), new(256, 128), new(128, 256)]));

            Assert.Equal([new(64, 448), new(128, 448), new(64, 384)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2 }.TerrainToIdMapPixel([new(128, 128), new(256, 128), new(128, 256)]));
            Assert.Equal([new(128, 896), new(256, 896), new(128, 768)], new Arma3MapConfigMock() { Size = 512, CellSize = new Vector2(2), Resolution = 2, IdMapMultiplier = 2 }.TerrainToIdMapPixel([new(128, 128), new(256, 128), new(128, 256)]));

        }

    }
}
