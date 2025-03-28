using BIS.WRP;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.Test.GameEngine;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class IdMapHelperTest
    {
        private const string RvMatA = @"class Stage3
{
    texture=""A_nopx.paa"";
	texGen=1;
};
class Stage4
{
    texture=""A_co.paa"";
	texGen=2;
};";

        private const string RvMatBC = @"class Stage3
{
    texture=""B_nopx.paa"";
	texGen=1;
};
class Stage4
{
    texture=""B_co.paa"";
	texGen=2;
};
class Stage5
{
    texture=""C_nopx.paa"";
	texGen=1;
};
class Stage6
{
    texture=""C_co.paa"";
	texGen=2;
};";

        private const string RvMatCB = @"class Stage3
{
    texture=""C_nopx.paa""; 
	texGen=1;
};
class Stage4
{
    texture=""C_co.paa""; 
	texGen=2;
};
class Stage5
{
    texture=""B_nopx.paa""; 
	texGen=1;
};
class Stage6
{
    texture=""B_co.paa""; 
	texGen=2;
};";

        [Fact]
        public async Task GetUsedTextureList_FromWrp_ReturnsEmptyList_WhenNoFilesFound()
        {
            var wrp = new EditableWrp
            {
                MatNames = new[] { "a.rvmat", "b.rvmat" }
            };
            var fs = new GameFileSystemMock();
            var result = await IdMapHelper.GetUsedTextureList(wrp, fs);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsedTextureList_FromWrp_ReturnsCorrectTextures()
        {
            var wrp = new EditableWrp
            {
                MatNames = new[] { "a.rvmat", "b.rvmat" }
            };
            var fs = new GameFileSystemMock();
            fs.WriteTextFile("a.rvmat", RvMatA);
            fs.WriteTextFile("b.rvmat", RvMatA);

            var result = await IdMapHelper.GetUsedTextureList(wrp, fs);

            Assert.Equal(new GroundDetailTexture("A_co.paa", "A_nopx.paa"), Assert.Single(result));
        }

        [Fact]
        public async Task GetUsedTextureList_FromWrp_ReturnsMultipleTextures()
        {
            var wrp = new EditableWrp
            {
                MatNames = new[] { "a.rvmat", "b.rvmat" }
            };
            var fs = new GameFileSystemMock();
            fs.WriteTextFile("a.rvmat", RvMatBC);
            fs.WriteTextFile("b.rvmat", RvMatCB);

            var result = await IdMapHelper.GetUsedTextureList(wrp, fs);

            Assert.Equal(2, result.Count);
            Assert.Contains(new GroundDetailTexture("B_co.paa", "B_nopx.paa"), result);
            Assert.Contains(new GroundDetailTexture("C_co.paa", "C_nopx.paa"), result);
        }

        [Fact]
        public void GetRvMatList_ReturnsEmptyList_WhenNoMatNames()
        {
            var wrp = new EditableWrp
            {
                MatNames = new string[] { }
            };

            var result = IdMapHelper.GetRvMatList(wrp);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsedTextureList_ReturnsEmptyList_WhenNoFilesFound()
        {
            var fs = new GameFileSystemMock();
            var result = await IdMapHelper.GetUsedTextureList(new List<string> { "a.rvmat", "b.rvmat" }, fs);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsedTextureList_ReturnsCorrectTextures()
        {
            var fs = new GameFileSystemMock();

            fs.WriteTextFile("a.rvmat", RvMatA);
            fs.WriteTextFile("b.rvmat", RvMatA);

            var result = await IdMapHelper.GetUsedTextureList(new List<string> { "a.rvmat", "b.rvmat" }, fs);

            Assert.Equal(new GroundDetailTexture("A_co.paa", "A_nopx.paa"), Assert.Single(result));
        }

        [Fact]
        public async Task GetUsedTextureList_ReturnsMultipleTextures()
        {
            var fs = new GameFileSystemMock();
            fs.WriteTextFile("a.rvmat", RvMatBC);
            fs.WriteTextFile("b.rvmat", RvMatCB);

            var result = await IdMapHelper.GetUsedTextureList(new List<string> { "a.rvmat", "b.rvmat" }, fs);

            Assert.Equal(2, result.Count);
            Assert.Contains(new GroundDetailTexture("B_co.paa", "B_nopx.paa"), result);
            Assert.Contains(new GroundDetailTexture("C_co.paa", "C_nopx.paa"), result);
        }
    }
}
