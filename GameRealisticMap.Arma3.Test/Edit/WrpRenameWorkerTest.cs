using BIS.WRP;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.Test.GameEngine;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpRenameWorkerTest
    {
        private const string OldConfig = @"class CfgPatches
{
	class arm_oldworldname
	{
		requiredAddons[] = { ""arm_centraleurope"" };
	};
};
class CfgWorldList
{
	class oldworldname{};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class oldworldname: arm_world_centraleurope
	{
		description = ""Some map name, GameRealisticMap"";
		worldName = ""oldprefix\oldworldname.wrp"";
		grma3_revision = 0;
        newRoadsShape = ""oldprefix\data\roads\roads.shp"";
        pictureMap = ""oldprefix\data\picturemap_ca.paa"";
        #include""inc01.hpp""
        #include ""inc02.hpp""
	};
};";
        private const string NewConfig = @"class CfgPatches
{
	class arm_oldworldname
	{
		requiredAddons[] = { ""arm_centraleurope"" };
	};
};
class CfgWorldList
{
	class newworldname{};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class newworldname: arm_world_centraleurope
	{
		description = ""Some map name, GameRealisticMap"";
		worldName = ""newprefix\newworldname.wrp"";
		grma3_revision = 0;
        newRoadsShape = ""newprefix\data\roads\roads.shp"";
        pictureMap = ""newprefix\data\picturemap_ca.paa"";
        #include""inc01.hpp""
        #include ""inc02.hpp""
	};
};";

        [Fact]
        public void UpdateConfigContent()
        {
            var fs = new GameFileSystemMock();
            var worker = new WrpRenameWorker(new NoProgressSystem(), fs, "oldprefix", "newprefix");

            var updated = worker.UpdateConfigContent(@" ""oldprefix\file01.paa"" ""other\file02.paa"" ""oldprefix\file03.paa"" ");

            Assert.Equal(@" ""newprefix\file01.paa"" ""other\file02.paa"" ""newprefix\file03.paa"" ", updated);
            Assert.Equal(2, worker.ReferencedFiles.Count());
            Assert.Contains(new("oldprefix\\file01.paa", "newprefix\\file01.paa"), worker.ReferencedFiles);
            Assert.Contains(new("oldprefix\\file03.paa", "newprefix\\file03.paa"), worker.ReferencedFiles);
        }

        [Fact]
        public async Task RenameAndCopyMaterials()
        {
            var fs = new GameFileSystemMock();
            fs.CreateDirectory("oldprefix\\data");
            fs.WriteTextFile("oldprefix\\data\\layer01.rvmat", @" ""oldprefix\file01.paa"" ""other\file02.paa"" ""oldprefix\file03.paa"" ");
            fs.WriteTextFile("oldprefix\\data\\layer02.rvmat", @" ""oldprefix\file04.paa"" ""other\file05.paa"" ""oldprefix\file06.paa"" ");
            var worker = new WrpRenameWorker(new NoProgressSystem(), fs, "oldprefix", "newprefix");
            var wrp = new EditableWrp();
            wrp.MatNames = new[] { null, "oldprefix\\data\\layer01.rvmat", "oldprefix\\data\\layer02.rvmat" };

            await worker.RenameAndCopyMaterials(wrp);

            Assert.Equal(new[] { null, "newprefix\\data\\layer01.rvmat", "newprefix\\data\\layer02.rvmat" }, wrp.MatNames);
            Assert.Equal(@" ""newprefix\file01.paa"" ""other\file02.paa"" ""newprefix\file03.paa"" ", fs.ReadAllText("newprefix\\data\\layer01.rvmat"));
            Assert.Equal(@" ""newprefix\file04.paa"" ""other\file05.paa"" ""newprefix\file06.paa"" ", fs.ReadAllText("newprefix\\data\\layer02.rvmat"));
            Assert.Equal(4, worker.ReferencedFiles.Count());
            Assert.Contains(new("oldprefix\\file01.paa", "newprefix\\file01.paa"), worker.ReferencedFiles);
            Assert.Contains(new("oldprefix\\file03.paa", "newprefix\\file03.paa"), worker.ReferencedFiles);
            Assert.Contains(new("oldprefix\\file04.paa", "newprefix\\file04.paa"), worker.ReferencedFiles);
            Assert.Contains(new("oldprefix\\file06.paa", "newprefix\\file06.paa"), worker.ReferencedFiles);
        }

        [Fact]
        public async Task UpdateConfig()
        {
            var fs = new GameFileSystemMock();
            fs.CreateDirectory("oldprefix");
            fs.WriteTextFile("oldprefix\\inc01.hpp", @" ""oldprefix\file01.paa"" ");
            fs.WriteTextFile("oldprefix\\inc02.hpp", @" ""other\file02.paa"" ");
            var worker = new WrpRenameWorker(new NoProgressSystem(), fs, "oldprefix", "newprefix");
            var oldConfig = GameConfigTextData.ReadFromContent(OldConfig, "oldworldname");
            Assert.Equal("oldworldname", oldConfig.WorldName);
            Assert.Equal("oldprefix", oldConfig.PboPrefix);
            Assert.Equal("oldprefix\\data\\roads", oldConfig.Roads);

            var newConfig = await worker.UpdateConfig(oldConfig, "newworldname");

            Assert.Equal("newworldname", newConfig.WorldName);
            Assert.Equal("newprefix", newConfig.PboPrefix);
            Assert.Equal("newprefix\\data\\roads", newConfig.Roads);
            Assert.Equal(NewConfig, newConfig.ToUpdatedContent());

            Assert.Equal(@" ""newprefix\file01.paa"" ", fs.ReadAllText("newprefix\\inc01.hpp"));
            Assert.Equal(@" ""other\file02.paa"" ", fs.ReadAllText("newprefix\\inc02.hpp"));
            Assert.Equal(NewConfig, fs.ReadAllText("newprefix\\config.cpp"));
            Assert.Equal(2, worker.ReferencedFiles.Count());
            Assert.Contains(new("oldprefix\\file01.paa", "newprefix\\file01.paa"), worker.ReferencedFiles); // from inc01.hpp
            Assert.Contains(new("oldprefix\\data\\picturemap_ca.paa", "newprefix\\data\\picturemap_ca.paa"), worker.ReferencedFiles); // from InitialConfig
        }

        [Fact]
        public async Task CopyReferencedFiles()
        {
            var fs = new GameFileSystemMock();
            fs.CreateDirectory("oldprefix");
            fs.WriteTextFile("oldprefix\\file01.paa", "FILE01");
            fs.WriteTextFile("oldprefix\\file03.paa", "FILE03");
            var worker = new WrpRenameWorker(new NoProgressSystem(), fs, "oldprefix", "newprefix");

            worker.UpdateConfigContent(@" ""oldprefix\file01.paa"" ""other\file02.paa"" ""oldprefix\file03.paa"" "); // Only to add values into ReferencedFiles

            Assert.Equal(2, worker.ReferencedFiles.Count());
           
            await worker.CopyReferencedFiles();

            Assert.Empty(worker.ReferencedFiles);

            Assert.Equal(@"FILE01", fs.ReadAllText("newprefix\\file01.paa"));
            Assert.Equal(@"FILE03", fs.ReadAllText("newprefix\\file03.paa"));
        }
    }
}
