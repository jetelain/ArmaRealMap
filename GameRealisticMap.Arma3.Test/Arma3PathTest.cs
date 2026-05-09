using GameRealisticMap.Arma3;

namespace GameRealisticMap.Arma3.Test
{
    public class Arma3PathTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(@"\ca\buildings\house.p3d", "house")]
        [InlineData("/ca/buildings/house.p3d", "house")]
        [InlineData(@"\ca/buildings\house.p3d", "house")]
        [InlineData("house.p3d", "house")]
        [InlineData(@"\ca\buildings\house", "house")]
        public void GetFileNameWithoutExtension(string? path, string? expected)
        {
            Assert.Equal(expected, Arma3Path.GetFileNameWithoutExtension(path));
        }
    }
}
