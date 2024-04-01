using System.Numerics;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Test.GameEngine.Materials
{
    public class ClutterConfigTest
    {
        [Fact]
        public void WriteTo()
        {
            var clutter = new ClutterConfig("name", 1, new ModelInfo("c","c.p3d", Vector3.Zero), 0.75, true, 0.9, 1.1);
            var sw = new StringWriter();
            clutter.WriteTo(sw);
            Assert.Equal(@"class name : DefaultClutter
{
	model=""c.p3d"";
	affectedByWind=0.75;
	swLighting=1;
	scaleMin=0.9;
	scaleMax=1.1;
};
", sw.ToString(), false, true);
        }
    }
}
