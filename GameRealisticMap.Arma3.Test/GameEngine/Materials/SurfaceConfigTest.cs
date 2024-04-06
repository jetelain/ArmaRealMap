using System.Numerics;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Test.GameEngine.Materials
{
    public class SurfaceConfigTest
    {
        [Fact]
        public void WriteCfgSurfacesTo()
        {
            var config = new SurfaceConfig("Gdt1", false, "color1", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, new List<ClutterConfig>(){
                            new ("C1", 0.5, new ModelInfo("c1","c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                            new ("C2", 0.5, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                        });
            var sw = new StringWriter();
            config.WriteCfgSurfacesTo(sw);
            Assert.Equal(@"
	class Gdt1 : Default
	{
        ACE_canDig=0;
		files=""color1"";
		character=""Gdt1Clutter"";
		soundEnviron=""env1"";
		soundHit=""hit1"";
		rough=1;
		maxSpeedCoef=2;
		dust=3;
		lucidity=4;
		grassCover=5;
		impact=""impact1"";
		surfaceFriction=6;
        maxClutterColoringCoef=7;
	};
", sw.ToString());
        }

        [Fact]
        public void WriteCfgSurfaceCharactersTo()
        {
            var config = new SurfaceConfig("Gdt1", false, "color1", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, new List<ClutterConfig>(){
                            new ("C1", 0.5, new ModelInfo("c1","c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                            new ("C2", 0.5, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                        });
            var sw = new StringWriter();
            config.WriteCfgSurfaceCharactersTo(sw);
            Assert.Equal(@"
    class Gdt1Clutter
	{
		probability[]={0.5,0.5};
		names[]={""C1"",""C2""};
	};
", sw.ToString());
        }
    }
}
