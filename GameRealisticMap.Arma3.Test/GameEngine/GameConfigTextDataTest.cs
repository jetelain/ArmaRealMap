using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.GameEngine;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class GameConfigTextDataTest
    {
        private const string OutOfGeneratorConfig = @"class CfgPatches
{
	class arm_m_30rxq741595s81
	{
		requiredAddons[] = { ""arm_centraleurope"" };
	};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class m_30rxq741595s81: arm_world_centraleurope
	{
		description = ""Some map name, GameRealisticMap"";
		worldName = ""z\arm\addons\m_30rxq741595s81\m_30rxq741595s81.wrp"";
        newRoadsShape = ""z\arm\addons\m_30rxq741595s81\data\roads\roads.shp"";
	};
};";

        private const string WithRevisionConfig = @"class CfgPatches
{
	class arm_m_30rxq741595s81
	{
		requiredAddons[] = { ""arm_centraleurope"" };
	};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class m_30rxq741595s81: arm_world_centraleurope
	{
		description = ""Some map name, GameRealisticMap"";
		worldName = ""z\arm\addons\m_30rxq741595s81\m_30rxq741595s81.wrp"";
		grma3_revision = 1234;
        newRoadsShape = ""z\arm\addons\m_30rxq741595s81\data\roads\roads.shp"";
	};
};";

        [Fact]

        public void ReadFromContent()
        {
            // Out of generator simplified config
            var config = GameConfigTextData.ReadFromContent(OutOfGeneratorConfig);
			Assert.Equal("m_30rxq741595s81", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81", config.PboPrefix);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81\\data\\roads", config.Roads);
            Assert.Equal(0, config.Revision);

            config = GameConfigTextData.ReadFromContent(OutOfGeneratorConfig, "m_30rxq741595s81");
            Assert.Equal("m_30rxq741595s81", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81", config.PboPrefix);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81\\data\\roads", config.Roads);
            Assert.Equal(0, config.Revision);

            config = GameConfigTextData.ReadFromContent(OutOfGeneratorConfig, "M_30RXQ741595s81");
            Assert.Equal("M_30RXQ741595s81", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81", config.PboPrefix);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81\\data\\roads", config.Roads);
            Assert.Equal(0, config.Revision);

            // WorldName mismatch
            config = GameConfigTextData.ReadFromContent(OutOfGeneratorConfig, "anotherworldname");
            Assert.Equal("anotherworldname", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal(string.Empty, config.PboPrefix);
            Assert.Equal(0, config.Revision);

            // With revision number
            config = GameConfigTextData.ReadFromContent(WithRevisionConfig);
            Assert.Equal("m_30rxq741595s81", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81", config.PboPrefix);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81\\data\\roads", config.Roads);
            Assert.Equal(1234, config.Revision);

            config = GameConfigTextData.ReadFromContent(WithRevisionConfig, "m_30rxq741595s81");
            Assert.Equal("m_30rxq741595s81", config.WorldName);
            Assert.Equal("Some map name, GameRealisticMap", config.Description);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81", config.PboPrefix);
            Assert.Equal("z\\arm\\addons\\m_30rxq741595s81\\data\\roads", config.Roads);
            Assert.Equal(1234, config.Revision);
        }
        [Fact]

        public void ToUpdatedContent()
        {
            // Edit revision number
            var config = GameConfigTextData.ReadFromContent(OutOfGeneratorConfig);
            config.Revision = 1234;
            Assert.Equal(WithRevisionConfig, config.ToUpdatedContent());

            // Edit description and revision number
            config.Description = "An other name";
            config.Revision = 6789;
            Assert.Equal(@"class CfgPatches
{
	class arm_m_30rxq741595s81
	{
		requiredAddons[] = { ""arm_centraleurope"" };
	};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class m_30rxq741595s81: arm_world_centraleurope
	{
		description = ""An other name"";
		worldName = ""z\arm\addons\m_30rxq741595s81\m_30rxq741595s81.wrp"";
		grma3_revision = 6789;
        newRoadsShape = ""z\arm\addons\m_30rxq741595s81\data\roads\roads.shp"";
	};
};", config.ToUpdatedContent());
        }
    }
}
