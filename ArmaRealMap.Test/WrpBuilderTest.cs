using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using Xunit;
using Xunit.Abstractions;

namespace ArmaRealMap.Test
{
    public class WrpBuilderTest
    {
        private readonly ITestOutputHelper output;

        public WrpBuilderTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Materials_Gossi()
        {
            var point = "30P XB 62600 87000";
            var gossi = new MapConfig() { CellSize = 10, GridSize = 8192, Resolution = 2, BottomLeft = point, PboPrefix = "prefix" };
            var wrp = WrpBuilder.InitWrp(gossi);
            WrpBuilder.SetMaterials(gossi, new TerrainTiler(gossi), wrp);
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers1.txt").ToList());
        }

        [Fact]
        public void Materials_Taunus()
        {
            var point = "30P XB 62600 87000";
            var taunus = new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 2, BottomLeft = point, TileSize = 512, PboPrefix = "prefix" };
            var wrp = WrpBuilder.InitWrp(taunus);
            WrpBuilder.SetMaterials(taunus, new TerrainTiler(taunus), wrp);
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers2.txt").ToList());
        }

        [Fact]
        public void Materials_Belfort()
        {
            var point = "30P XB 62600 87000";
            var taunus = new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 1, BottomLeft = point, TileSize = 1024, PboPrefix = "prefix" };
            var wrp = WrpBuilder.InitWrp(taunus);
            WrpBuilder.SetMaterials(taunus, new TerrainTiler(taunus), wrp);
            Assert.Equal(wrp.MaterialIndex.Select(x => wrp.MatNames[x]).ToList(), Read("layers2.txt").ToList());
        }

        private IEnumerable<string> Read(string v)
        {
            using (var reader = new StreamReader(typeof(WrpBuilderTest).Assembly.GetManifestResourceStream("ArmaRealMap.Test." + v)))
            {
                string l;
                while( (l = reader.ReadLine()) != null )
                {
                    yield return l;
                }
            }
        }
    }
}
