using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.Assets
{
    public class TerrainMaterialLibraryTest
    {
        [Fact]
        public void GetMaterialById()
        {
            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>() { 
                new TerrainMaterialDefinition(new TerrainMaterial("a_nopx.paa", "a_co.paa", new Rgb24(128,128,128), null), new TerrainMaterialUsage[0]),
                new TerrainMaterialDefinition(new TerrainMaterial("b_nopx.paa", "b_co.paa", new Rgb24(255, 0, 0), null), new TerrainMaterialUsage[0]),
                new TerrainMaterialDefinition(new TerrainMaterial("c_nopx.paa", "c_co.paa", new Rgb24(0, 255, 0), null), new TerrainMaterialUsage[0]),
                new TerrainMaterialDefinition(new TerrainMaterial("d_nopx.paa", "d_co.paa", new Rgb24(0, 0, 255), null), new TerrainMaterialUsage[0])
            });
            
            // Exact match
            Assert.Equal("a_co.paa", lib.GetMaterialById(new Rgb24(128, 128, 128)).ColorTexture);
            Assert.Equal("b_co.paa", lib.GetMaterialById(new Rgb24(255, 0, 0)).ColorTexture);
            Assert.Equal("c_co.paa", lib.GetMaterialById(new Rgb24(0, 255, 0)).ColorTexture);
            Assert.Equal("d_co.paa", lib.GetMaterialById(new Rgb24(0, 0, 255)).ColorTexture);
            
            // Approximate match
            Assert.Equal("b_co.paa", lib.GetMaterialById(new Rgb24(192, 0, 0)).ColorTexture);
            Assert.Equal("c_co.paa", lib.GetMaterialById(new Rgb24(0, 192, 0)).ColorTexture);
            Assert.Equal("d_co.paa", lib.GetMaterialById(new Rgb24(0, 0, 192)).ColorTexture);
        }

        private static TerrainMaterialLibrary CreateLibrary()
        {
            return new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>()
            {
                new TerrainMaterialDefinition(new TerrainMaterial("def_nopx.paa", "def_co.paa", new Rgb24(0, 0, 0), null),
                    new[] { TerrainMaterialUsage.Default }),
                new TerrainMaterialDefinition(new TerrainMaterial("grass_nopx.paa", "grass_co.paa", new Rgb24(0, 128, 0), null),
                    new[] { TerrainMaterialUsage.Grass }),
                new TerrainMaterialDefinition(new TerrainMaterial("meadow_nopx.paa", "meadow_co.paa", new Rgb24(0, 64, 0), null),
                    new[] { TerrainMaterialUsage.Meadow }),
                new TerrainMaterialDefinition(new TerrainMaterial("rock_nopx.paa", "rock_co.paa", new Rgb24(128, 128, 128), null),
                    new[] { TerrainMaterialUsage.RockGround }),
            });
        }

        [Fact]
        public void GetMaterialByUsage_ExactMatch_ReturnsCorrectMaterial()
        {
            var lib = CreateLibrary();
            Assert.Equal("grass_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.Grass).ColorTexture);
            Assert.Equal("meadow_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.Meadow).ColorTexture);
            Assert.Equal("def_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.Default).ColorTexture);
        }

        [Fact]
        public void GetMaterialByUsage_UnknownUsage_FallsBackToDefault()
        {
            var lib = CreateLibrary();
            // Sand not in library, should fall back to default
            Assert.Equal("def_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.Sand).ColorTexture);
        }

        [Fact]
        public void GetMaterialByUsage_ScreeSurface_FallsBackToRockGround()
        {
            var lib = CreateLibrary();
            Assert.Equal("rock_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.ScreeSurface).ColorTexture);
        }

        [Fact]
        public void GetSurface_ReturnsSurface()
        {
            // Files pattern "grass_co" matches ColorTexture filename "grass_co.paa" (without extension)
            var surface = new SurfaceConfig("GdtGrass", false, "grass_co", "env", "hit", 1, 2, 3, 4, 5, "impact", 6, 7,
                new List<ClutterConfig>() { new("C1", 0.5, new ModelInfo("c1", "c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2) });
            var data = new TerrainMaterialData(TerrainMaterialDataFormat.PAA, Array.Empty<byte>(), Array.Empty<byte>());

            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>()
            {
                new TerrainMaterialDefinition(new TerrainMaterial("def_nopx.paa", "def_co.paa", new Rgb24(0, 0, 0), null),
                    new[] { TerrainMaterialUsage.Default }),
                new TerrainMaterialDefinition(new TerrainMaterial("grass_nopx.paa", "grass_co.paa", new Rgb24(0, 128, 0), null),
                    new[] { TerrainMaterialUsage.Grass }, surface, data),
            });

            var mat = lib.GetMaterialByUsage(TerrainMaterialUsage.Grass);
            Assert.Equal(surface, lib.GetSurface(mat));
        }

        [Fact]
        public void TextureSizeInMeters_DefaultIsCorrect()
        {
            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>());
            Assert.Equal(TerrainMaterialLibrary.DefaultTextureSizeInMeters, lib.TextureSizeInMeters);
        }

        [Fact]
        public void TextureSizeInMeters_CustomValue()
        {
            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>(), textureSizeInMeters: 8.0);
            Assert.Equal(8.0, lib.TextureSizeInMeters);
        }

        [Fact]
        public void DuplicateUsage_LastDefinitionWins()
        {
            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>()
            {
                new TerrainMaterialDefinition(new TerrainMaterial("first_nopx.paa", "first_co.paa", new Rgb24(10, 10, 10), null),
                    new[] { TerrainMaterialUsage.Grass }),
                new TerrainMaterialDefinition(new TerrainMaterial("second_nopx.paa", "second_co.paa", new Rgb24(20, 20, 20), null),
                    new[] { TerrainMaterialUsage.Grass }),
            });
            Assert.Equal("second_co.paa", lib.GetMaterialByUsage(TerrainMaterialUsage.Grass).ColorTexture);
        }
    }
}
