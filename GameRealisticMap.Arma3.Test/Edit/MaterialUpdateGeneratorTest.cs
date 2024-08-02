using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.Test.GameEngine;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class MaterialUpdateGeneratorTest
    {
        [Fact]
        public async Task Replace()
        {
            var fs = new GameFileSystemMock();
            var gen = new MaterialUpdateGenerator(new NoProgress(), fs);
            fs.WriteTextFile("test.rvmat", 
@"class Stage3
{
	texture=""A_nopx.paa"";
	texGen=1;
};
class Stage4
{
	texture=""A_co.paa"";
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
};
class Stage7
{
	texture=""A_nopx.paa"";
	texGen=1;
};
class Stage8
{
	texture=""A_co.paa"";
	texGen=2;
};");

			await gen.Replace("test.rvmat", "a_co.paa", new Arma3.Assets.TerrainMaterial("C_nopx.paa", "C_co.paa", new SixLabors.ImageSharp.PixelFormats.Rgb24(), null), new TestMapConfig());

			Assert.Equal(@"class Stage3
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
};
class Stage7
{
	texture=""C_nopx.paa"";
	texGen=1;
};
class Stage8
{
	texture=""C_co.paa"";
	texGen=2;
};", fs.ReadAllText("test.rvmat"));
        }
    }
}
