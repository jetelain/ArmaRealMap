using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.Weather;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class GameConfigGeneratorTest
    {

        /// <summary>
        /// Simplest test case
        /// </summary>
        [Fact]
        public void Generate()
        {
            var writer = new GameFileSystemMock();
            var assets = new Arma3Assets()
            {
                Materials = new TerrainMaterialLibrary( new (){ new TerrainMaterialDefinition(new TerrainMaterial("defNormal", "defColor", new Rgb24(), null), new[] { TerrainMaterialUsage.Default }) } )
            };
            var config = new TestMapConfig();
            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", config.GridCellSize, config.GridSize);
            var generator = new GameConfigGenerator(assets, writer);
            var context = new ContextMock
            {
                new CitiesData(new List<City>()),
                new OceanData(new List<Geometries.TerrainPolygon>(), new List<Geometries.TerrainPolygon>(), false),
                new ElevationOutOfBoundsData(new ElevationMinMax[0]),
                new WeatherData(null)
            };
            generator.Generate(new TestMapConfig(), context, area);
            writer.AssertFiles<GameConfigGeneratorTest>(nameof(Generate));
        }

        /// <summary>
        /// Clutter
        /// </summary>
        [Fact]
        public void Generate_Clutter()
        {
            var writer = new GameFileSystemMock();
            var assets = new Arma3Assets()
            {
                Materials = new TerrainMaterialLibrary(new() { 
                    new TerrainMaterialDefinition(new TerrainMaterial("defNormal", "defColor", new Rgb24(0,0,0), null), new[] { TerrainMaterialUsage.Default }),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal1", "color1", new Rgb24(32,32,32), null), new TerrainMaterialUsage[0], 
                        new SurfaceConfig("Gdt1", false, "color1", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, new List<ClutterConfig>(){ 
                            new ("C1", 0.5, new ModelInfo("c1","c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                            new ("C2", 0.5, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                        })),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal2", "color2", new Rgb24(64,64,64), null), new TerrainMaterialUsage[0],
                        new SurfaceConfig("Gdt2", true, "color2", "env2", "hit2", 8, 9, 10, 11, 12, "impact2", 13, 14, new List<ClutterConfig>(){
                            new ("C2", 0.25, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                            new ("C3", 0.75, new ModelInfo("c3","c3.p3d", Vector3.Zero), 1, true, 0.8, 1.2)
                        }))
                })
            };
            var config = new TestMapConfig();
            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", config.GridCellSize, config.GridSize);
            var generator = new GameConfigGenerator(assets, writer);
            var context = new ContextMock
            {
                new CitiesData(new List<City>()),
                new OceanData(new List<Geometries.TerrainPolygon>(), new List<Geometries.TerrainPolygon>(), false),
                new ElevationOutOfBoundsData(new ElevationMinMax[0]),
                new WeatherData(null)
            };
            generator.Generate(new TestMapConfig(), context, area);
            writer.AssertFiles<GameConfigGeneratorTest>(nameof(Generate_Clutter));
        }

        /// <summary>
        /// Surfaces
        /// </summary>
        [Fact]
        public void Generate_Surfaces()
        {
            var writer = new GameFileSystemMock();
            var assets = new Arma3Assets()
            {
                Materials = new TerrainMaterialLibrary(new() {
                    new TerrainMaterialDefinition(new TerrainMaterial("defNormal", "defColor", new Rgb24(0,0,0), null), new[] { TerrainMaterialUsage.Default }),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal1", "color1", new Rgb24(32,32,32), null), new TerrainMaterialUsage[0],
                        new SurfaceConfig("Gdt1", false, "color1", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, new List<ClutterConfig>(){
                            new ("C1", 0.5, new ModelInfo("c1","c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                            new ("C2", 0.5, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                        }), 
                        new TerrainMaterialData(TerrainMaterialDataFormat.PAA, new byte[0], new byte[0])),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal2", "color2", new Rgb24(64,64,64), null), new TerrainMaterialUsage[0],
                        new SurfaceConfig("Gdt2", true, "color2", "env2", "hit2", 8, 9, 10, 11, 12, "impact2", 13, 14, new List<ClutterConfig>(){
                            new ("C2", 0.25, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                            new ("C3", 0.75, new ModelInfo("c3","c3.p3d", Vector3.Zero), 1, true, 0.8, 1.2)
                        }), 
                        new TerrainMaterialData(TerrainMaterialDataFormat.PAA, new byte[0], new byte[0]))
                })
            };
            var config = new TestMapConfig();
            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", config.GridCellSize, config.GridSize);
            var generator = new GameConfigGenerator(assets, writer);
            var context = new ContextMock
            {
                new CitiesData(new List<City>()),
                new OceanData(new List<Geometries.TerrainPolygon>(), new List<Geometries.TerrainPolygon>(), false),
                new ElevationOutOfBoundsData(new ElevationMinMax[0]),
                new WeatherData(null)
            };
            generator.Generate(new TestMapConfig(), context, area);
            writer.AssertFiles<GameConfigGeneratorTest>(nameof(Generate_Surfaces));
        }
    }
}
