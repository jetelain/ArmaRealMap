using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;
using SixLabors.ImageSharp.PixelFormats;

// Note: TerrainMaterialDefinition validates Surface.Files against ColorTexture filename in DEBUG.
// Tests use matching patterns (e.g. surface Files="grass_co" for texture "grass_co.paa").

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class MaterialConfigGeneratorTest
    {
        private static ClutterConfig MakeClutter(string name, string model, double probability = 0.5)
            => new ClutterConfig(name, probability, new ModelInfo(name, model, Vector3.Zero), 1, false, 0.8, 1.2);

        private static SurfaceConfig MakeSurface(string name, string files, List<ClutterConfig> clutters)
            => new SurfaceConfig(name, false, files, "env", "hit", 1, 2, 3, 4, 5, "impact", 6, 7, clutters);

        [Fact]
        public void GenerateClutters_EmptyInput_ReturnsEmptyString()
        {
            var result = MaterialConfigGenerator.GenerateClutters(Enumerable.Empty<SurfaceConfig>());
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GenerateClutters_SingleSurface_WritesAllClutters()
        {
            var surface = MakeSurface("Gdt1", "color*",
                new List<ClutterConfig> { MakeClutter("C1", "c1.p3d"), MakeClutter("C2", "c2.p3d") });

            var result = MaterialConfigGenerator.GenerateClutters(new[] { surface });

            Assert.Contains("class C1 : DefaultClutter", result);
            Assert.Contains("class C2 : DefaultClutter", result);
            Assert.Contains("c1.p3d", result);
            Assert.Contains("c2.p3d", result);
        }

        [Fact]
        public void GenerateClutters_DuplicateClutterName_WrittenOnlyOnce()
        {
            var surface1 = MakeSurface("Gdt1", "color1", new List<ClutterConfig> { MakeClutter("SharedClutter", "c1.p3d") });
            var surface2 = MakeSurface("Gdt2", "color2", new List<ClutterConfig> { MakeClutter("SharedClutter", "c2.p3d") });

            var result = MaterialConfigGenerator.GenerateClutters(new[] { surface1, surface2 });

            // "SharedClutter" should appear only once
            var count = result.Split("class SharedClutter").Length - 1;
            Assert.Equal(1, count);
        }

        [Fact]
        public void GenerateSurfaces_NoSurfaces_ReturnsEmptyString()
        {
            var definitions = new List<TerrainMaterialDefinition>
            {
                new TerrainMaterialDefinition(new TerrainMaterial("n.paa", "c.paa", new Rgb24(), null),
                    new[] { TerrainMaterialUsage.Default })
            };
            var result = MaterialConfigGenerator.GenerateSurfaces(definitions);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GenerateSurfaces_WithSurface_ContainsCfgSurfacesAndCharacters()
        {
            // Files pattern must match ColorTexture filename (grass_co) for the constructor validation
            var surface = MakeSurface("GdtGrass", "grass_co",
                new List<ClutterConfig> { MakeClutter("C1", "c1.p3d") });
            var data = new TerrainMaterialData(TerrainMaterialDataFormat.PAA, Array.Empty<byte>(), Array.Empty<byte>());

            var definitions = new List<TerrainMaterialDefinition>
            {
                new TerrainMaterialDefinition(new TerrainMaterial("grass_nopx.paa", "grass_co.paa", new Rgb24(0, 128, 0), null),
                    new[] { TerrainMaterialUsage.Grass }, surface, data)
            };

            var result = MaterialConfigGenerator.GenerateSurfaces(definitions);

            Assert.Contains("class CfgSurfaces", result);
            Assert.Contains("class CfgSurfaceCharacters", result);
            Assert.Contains("class GdtGrass", result);
        }

        [Fact]
        public void GenerateConfigFiles_WritesClutterAndSurfaceFiles()
        {
            var surface = MakeSurface("GdtGrass", "grass_co",
                new List<ClutterConfig> { MakeClutter("C1", "c1.p3d") });
            var data = new TerrainMaterialData(TerrainMaterialDataFormat.PAA, Array.Empty<byte>(), Array.Empty<byte>());

            var lib = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>
            {
                new TerrainMaterialDefinition(new TerrainMaterial("grass_nopx.paa", "grass_co.paa", new Rgb24(0, 128, 0), null),
                    new[] { TerrainMaterialUsage.Grass }, surface, data)
            });

            var writer = new GameFileSystemMock();
            writer.CreateDirectory("z\\arm\\addons\\arm_testworld");
            MaterialConfigGenerator.GenerateConfigFiles(writer, new TestMapConfig(), lib);

            Assert.True(writer.TextFiles.ContainsKey("z\\arm\\addons\\arm_testworld\\clutter.hpp"));
            Assert.True(writer.TextFiles.ContainsKey("z\\arm\\addons\\arm_testworld\\surfaces.hpp"));

            Assert.Contains("class C1 : DefaultClutter", writer.TextFiles["z\\arm\\addons\\arm_testworld\\clutter.hpp"]);
            Assert.Contains("class CfgSurfaces", writer.TextFiles["z\\arm\\addons\\arm_testworld\\surfaces.hpp"]);
        }
    }
}
