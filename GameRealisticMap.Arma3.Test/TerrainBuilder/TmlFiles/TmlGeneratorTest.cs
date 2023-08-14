using GameRealisticMap.Arma3.TerrainBuilder.TmlFiles;

namespace GameRealisticMap.Arma3.Test.TerrainBuilder.TmlFiles
{
    public class TmlGeneratorTest
    {
        [Fact]
        public void GetBundle()
        {
            Assert.Equal("arma3", TmlGenerator.GetBundle(@"a3\plants_f\Bush\b_ArundoD2s_F.p3d"));
            Assert.Equal("EMOGLOBINSKY", TmlGenerator.GetBundle(@"EMOGLOBINSKY\EM_buildings\em_building_02.p3d"));
            Assert.Equal("rhsterracore", TmlGenerator.GetBundle(@"rhsterracore\addons\rhs_structures_civilian\multistory\hruschevka_9F\rhs_hruschevka_9F_01.p3d"));
            Assert.Equal("CUP_Terrains_cup_terrains_winter_objects_Billboards", TmlGenerator.GetBundle(@"CUP\Terrains\cup_terrains_winter_objects\Billboards\s_billboard_1_ep1.p3d"));
            Assert.Equal("arm", TmlGenerator.GetBundle(@"z\arm\addons\common_v2\data\water\arm_pond_blue_5.p3d"));
            Assert.Equal("none", TmlGenerator.GetBundle(@"helloworld.p3d"));
        }
    }
}
