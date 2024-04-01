using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Arma3.Test.Assets
{
    public class TerrainMaterialTest
    {
        [Fact]
        public void GetNormalTexturePath()
        {
            Assert.Equal("Material", new TerrainMaterial("Material", "", new SixLabors.ImageSharp.PixelFormats.Rgb24(), null).GetNormalTexturePath(new TestMapConfig()));
            Assert.Equal("z\\arm\\addons\\arm_testworld\\Material", new TerrainMaterial("{PboPrefix}\\Material", "", new SixLabors.ImageSharp.PixelFormats.Rgb24(), null).GetNormalTexturePath(new TestMapConfig()));
        }

        [Fact]
        public void GetColorTexturePath()
        {
            Assert.Equal("Material", new TerrainMaterial("", "Material", new SixLabors.ImageSharp.PixelFormats.Rgb24(), null).GetColorTexturePath(new TestMapConfig()));
            Assert.Equal("z\\arm\\addons\\arm_testworld\\Material", new TerrainMaterial("", "{PboPrefix}\\Material", new SixLabors.ImageSharp.PixelFormats.Rgb24(), null).GetColorTexturePath(new TestMapConfig()));
        }
    }
}
