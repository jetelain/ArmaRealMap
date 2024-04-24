using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.Test.GameEngine;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class IdMapHelperTest
    {

        [Fact]
        public async Task GetUsedTextureList()
        {
            var fs = new GameFileSystemMock();

            // Files not found
            var result = await IdMapHelper.GetUsedTextureList(new List<string>() { "a.rvmat", "b.rvmat" }, fs);
            Assert.Empty(result);

            // One texture
            var contentA = @"class Stage3
{
	texture=""A_nopx.paa"";
	texGen=1;
};
class Stage4
{
	texture=""A_co.paa"";
	texGen=2;
};";
            fs.WriteTextFile("a.rvmat", contentA); 
            fs.WriteTextFile("b.rvmat", contentA);
            result = await IdMapHelper.GetUsedTextureList(new List<string>() { "a.rvmat", "b.rvmat" }, fs);
            Assert.Equal(new GroundDetailTexture("A_co.paa", "A_nopx.paa"), Assert.Single(result));


            fs.WriteTextFile("a.rvmat", @"class Stage3
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
};");
            fs.WriteTextFile("b.rvmat", @"class Stage3
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
};");
            result = await IdMapHelper.GetUsedTextureList(new List<string>() { "a.rvmat", "b.rvmat" }, fs);
            Assert.Equal(2, result.Count);
            Assert.Contains(new GroundDetailTexture("B_co.paa", "B_nopx.paa"), result);
            Assert.Contains(new GroundDetailTexture("C_co.paa", "C_nopx.paa"), result);
        }
    }
}
