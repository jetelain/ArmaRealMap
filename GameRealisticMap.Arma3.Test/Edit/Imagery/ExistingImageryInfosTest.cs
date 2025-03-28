using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.IO;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class ExistingImageryInfosTest
    {
        [Fact]
        public void TryCreate_ReturnsNull_WhenFilesDoNotExist()
        {
            var projectDriveMock = new Mock<IProjectDrive>();
            projectDriveMock.Setup(pd => pd.GetFullPath(It.IsAny<string>())).Returns((string path) => "nosuchfile");

            var result = ExistingImageryInfos.TryCreate(projectDriveMock.Object, "testPrefix", 1000f);

            Assert.Null(result);
        }

        [Fact]
        public void TryCreate_ReturnsNull_WhenRegexDoesNotMatch()
        {
            var projectDriveMock = new Mock<IProjectDrive>();
            projectDriveMock.Setup(pd => pd.GetFullPath(It.IsAny<string>())).Returns((string path) => path.Substring(path.LastIndexOf('\\') + 1));

            var img = new Image<Rgba32>(1024, 1024);
            img.Save("M_000_000_lca.png");
            img.Save("S_000_000_lco.png");
            File.WriteAllText("P_000-000.rvmat", "invalid");

            var result = ExistingImageryInfos.TryCreate(projectDriveMock.Object, "testPrefix", 1000f);

            Assert.Null(result);

            File.Delete("M_000_000_lca.png");
            File.Delete("S_000_000_lco.png");
            File.Delete("P_000-000.rvmat");
        }

        [Fact]
        public void TryCreate_ReturnsExistingImageryInfos_WhenFilesExistAndRegexMatches()
        {
            var projectDriveMock = new Mock<IProjectDrive>();
            projectDriveMock.Setup(pd => pd.GetFullPath(It.IsAny<string>())).Returns((string path) => "1x"+path.Substring(path.LastIndexOf('\\')+1));

            using var img = new Image<Rgba32>(1024, 1024);
            img.Save("1xM_000_000_lca.png");
            img.Save("1xS_000_000_lco.png");
            File.WriteAllText("1xP_000-000.rvmat", "aside[]={0.0009765625,");

            var result = ExistingImageryInfos.TryCreate(projectDriveMock.Object, "testPrefix", 1000f);

            Assert.NotNull(result);
            Assert.Equal(1024, result.TileSize);
            Assert.Equal(1d, result.Resolution);
            Assert.Equal(1000f, result.SizeInMeters);
            Assert.Equal("testPrefix", result.PboPrefix);
            Assert.Equal(1, result.IdMapMultiplier);

            File.Delete("1xM_000_000_lca.png");
            File.Delete("1xS_000_000_lco.png");
            File.Delete("1xP_000-000.rvmat");
        }


        [Fact]
        public void TryCreate_ReturnsExistingImageryInfos_WhenFilesExistAndRegexMatchesAndIdMapHasMultiplier()
        {
            var projectDriveMock = new Mock<IProjectDrive>();
            projectDriveMock.Setup(pd => pd.GetFullPath(It.IsAny<string>())).Returns((string path) => "2x"+path.Substring(path.LastIndexOf('\\') + 1));

            using var img = new Image<Rgba32>(2048, 2048);
            img.Save("2xM_000_000_lca.png");
            using var img2 = new Image<Rgba32>(1024, 1024);
            img2.Save("2xS_000_000_lco.png");

            File.WriteAllText("2xP_000-000.rvmat", "aside[]={0.0009765625,");

            var result = ExistingImageryInfos.TryCreate(projectDriveMock.Object, "testPrefix", 1000f);

            Assert.NotNull(result);
            Assert.Equal(1024, result.TileSize);
            Assert.Equal(1d, result.Resolution);
            Assert.Equal(1000f, result.SizeInMeters);
            Assert.Equal("testPrefix", result.PboPrefix);
            Assert.Equal(2, result.IdMapMultiplier);

            File.Delete("2xM_000_000_lca.png");
            File.Delete("2xS_000_000_lco.png");
            File.Delete("2xP_000-000.rvmat");
        }
    }
}
