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

        private static SurfaceConfig MakeSurface(string files)
            => new SurfaceConfig("Name", false, files, "env", "hit", 1, 2, 3, 4, 5, "impact", 6, 7, new List<ClutterConfig>());

        [Theory]
        [InlineData("grass_co", "grass_co", true)]
        [InlineData("GRASS_CO", "grass_co", true)]       // case-insensitive exact
        [InlineData("grass_co", "GRASS_CO", true)]       // reverse case
        [InlineData("grass_co", "sand_co", false)]       // different name
        [InlineData("grass*", "grass_co", true)]         // wildcard prefix match
        [InlineData("grass*", "grasslands_co", true)]    // wildcard, longer match
        [InlineData("grass*", "GRASS_CO", true)]         // wildcard, case-insensitive
        [InlineData("grass*", "sand_co", false)]         // wildcard, no match
        public void Match(string files, string fileName, bool expected)
        {
            var surface = MakeSurface(files);
            Assert.Equal(expected, surface.Match(fileName));
        }

        [Fact]
        public void WithNameAndFiles_ReturnsNewSurfaceWithUpdatedNameAndFiles()
        {
            var clutters = new List<ClutterConfig>
            {
                new ClutterConfig("C1", 0.5, new Arma3.TerrainBuilder.ModelInfo("c1", "c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2)
            };
            var original = new SurfaceConfig("OrigName", true, "orig_files*", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, clutters);

            var result = original.WithNameAndFiles("NewName", "new_files");

            Assert.Equal("NewName", result.Name);
            Assert.Equal("new_files", result.Files);

            // All other properties should be preserved
            Assert.Equal(original.AceCanDig, result.AceCanDig);
            Assert.Equal(original.SoundEnviron, result.SoundEnviron);
            Assert.Equal(original.SoundHit, result.SoundHit);
            Assert.Equal(original.Rough, result.Rough);
            Assert.Equal(original.MaxSpeedCoef, result.MaxSpeedCoef);
            Assert.Equal(original.Dust, result.Dust);
            Assert.Equal(original.Lucidity, result.Lucidity);
            Assert.Equal(original.GrassCover, result.GrassCover);
            Assert.Equal(original.Impact, result.Impact);
            Assert.Equal(original.SurfaceFriction, result.SurfaceFriction);
            Assert.Equal(original.MaxClutterColoringCoef, result.MaxClutterColoringCoef);
        }

        [Fact]
        public void WithNameAndFiles_ClutterNamesArePrefixed()
        {
            var clutters = new List<ClutterConfig>
            {
                new ClutterConfig("Clutter1", 0.5, new Arma3.TerrainBuilder.ModelInfo("c1", "c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                new ClutterConfig("Clutter2", 0.5, new Arma3.TerrainBuilder.ModelInfo("c2", "c2.p3d", Vector3.Zero), 1, false, 0.8, 1.2)
            };
            var original = new SurfaceConfig("OrigName", false, "orig_files", "env", "hit", 1, 2, 3, 4, 5, "impact", 6, 7, clutters);

            var result = original.WithNameAndFiles("NewName", "new_files");

            Assert.Equal(2, result.Character.Count);
            Assert.Equal("NewNameClutter1", result.Character[0].Name);
            Assert.Equal("NewNameClutter2", result.Character[1].Name);
        }
    }
}
