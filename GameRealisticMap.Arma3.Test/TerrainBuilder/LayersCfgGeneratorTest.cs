using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.Test.GameEngine;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.TerrainBuilder
{
    public class LayersCfgGeneratorTest
    {

        [Fact]
        public void Generate()
        {
            var materials = new TerrainMaterialLibrary(new() {
                    new TerrainMaterialDefinition(new TerrainMaterial("defNormal", "defColor", new Rgb24(0,0,0), null), new[] { TerrainMaterialUsage.Default }),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal1", "color1", new Rgb24(32,32,32), null), new[] { TerrainMaterialUsage.Meadow },
                        new SurfaceConfig("Gdt1", false, "color1", "env1", "hit1", 1, 2, 3, 4, 5, "impact1", 6, 7, new List<ClutterConfig>(){
                            new ("C1", 0.5, new ModelInfo("c1","c1.p3d", Vector3.Zero), 1, false, 0.8, 1.2),
                            new ("C2", 0.5, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                        })),
                    new TerrainMaterialDefinition(new TerrainMaterial("normal2", "color2", new Rgb24(64,64,64), null), new[] { TerrainMaterialUsage.Grass },
                        new SurfaceConfig("Gdt2", true, "color2", "env2", "hit2", 8, 9, 10, 11, 12, "impact2", 13, 14, new List<ClutterConfig>(){
                            new ("C2", 0.25, new ModelInfo("c2","c2.p3d", Vector3.Zero), 0.5, false, 0.9, 1.1),
                            new ("C3", 0.75, new ModelInfo("c3","c3.p3d", Vector3.Zero), 1, true, 0.8, 1.2)
                        }))
                });
            var writer = new GameFileSystemMock();
            var generator = new LayersCfgGenerator(materials, new NoProgressSystem(), writer);

            generator.WriteLayersCfg(new TestMapConfig());
            writer.AssertFiles<LayersCfgGeneratorTest>(nameof(Generate));
        }
    }
}
