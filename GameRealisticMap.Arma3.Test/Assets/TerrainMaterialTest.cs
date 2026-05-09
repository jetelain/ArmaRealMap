using GameRealisticMap.Arma3.Assets;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.Assets
{
    public class TerrainMaterialTest
    {
        [Fact]
        public void GetNormalTexturePath_ReplacesPboPrefix()
        {
            var mat = new TerrainMaterial("{PboPrefix}\\nopx.paa", "{PboPrefix}\\co.paa", new Rgb24(), null);
            var config = new TestMapConfig();

            Assert.Equal("z\\arm\\addons\\arm_testworld\\nopx.paa", mat.GetNormalTexturePath(config));
        }

        [Fact]
        public void GetNormalTexturePath_NoPlaceholder_ReturnsOriginal()
        {
            var mat = new TerrainMaterial("data\\nopx.paa", "data\\co.paa", new Rgb24(), null);
            var config = new TestMapConfig();

            Assert.Equal("data\\nopx.paa", mat.GetNormalTexturePath(config));
        }

        [Fact]
        public void GetColorTexturePath_ReplacesPboPrefix()
        {
            var mat = new TerrainMaterial("{PboPrefix}\\nopx.paa", "{PboPrefix}\\co.paa", new Rgb24(), null);
            var config = new TestMapConfig();

            Assert.Equal("z\\arm\\addons\\arm_testworld\\co.paa", mat.GetColorTexturePath(config));
        }

        [Fact]
        public void GetColorTexturePath_NoPlaceholder_ReturnsOriginal()
        {
            var mat = new TerrainMaterial("data\\nopx.paa", "data\\co.paa", new Rgb24(), null);
            var config = new TestMapConfig();

            Assert.Equal("data\\co.paa", mat.GetColorTexturePath(config));
        }
    }
}
