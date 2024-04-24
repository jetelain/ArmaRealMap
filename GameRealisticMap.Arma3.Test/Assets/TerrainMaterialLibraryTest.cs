using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
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
    }
}
