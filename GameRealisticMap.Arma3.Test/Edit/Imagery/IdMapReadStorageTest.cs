using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.Test.GameEngine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class IdMapReadStorageTest
    {
        private const string RvMat = @"class Stage3
{
    texture=""normal2"";
	texGen=1;
};
class Stage4
{
    texture=""color2"";
	texGen=2;
};";

        [Fact]
        public async Task LoadImagePart_ValidFile_ReturnsImage()
        {
            var fileSystem = new GameFileSystemMock();
            fileSystem.CreateDirectory("prefix\\data\\layers");
            fileSystem.WritePngImage("prefix\\data\\layers\\M_000_000_lca.png", new Image<Rgba32>(256, 256));
            fileSystem.WriteTextFile("prefix\\data\\layers\\P_000-000.rvmat", RvMat);

            var library = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>
            {
                new TerrainMaterialDefinition(new TerrainMaterial("normal1", "color1", new Rgb24(1, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal2", "color2", new Rgb24(2, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal3", "color3", new Rgb24(3, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal4", "color4", new Rgb24(4, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal5", "color5", new Rgb24(5, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal6", "color6", new Rgb24(6, 0, 0), null),[])
            });

            var partitioner = new ImageryPartitionerMock();

            var storage = new IdMapReadStorage(partitioner, fileSystem, "prefix", library, Arma3MapConfigMock.Island512);

            var result = await storage.LoadImagePart<Rgba32>(1);

            Assert.NotNull(result);
            Assert.Equal(256, result.Width);
            Assert.Equal(256, result.Height);
            Assert.Equal(new Rgba32(2, 0, 0, 255), result[0,0]);
        }

        [Fact]
        public void GetColor_ValidInput_ReturnsCorrectColor()
        {
            var textures = new List<Rgb24>
                {
                    new Rgb24(1, 0, 0),
                    new Rgb24(2, 0, 0),
                    new Rgb24(3, 0, 0),
                    new Rgb24(4, 0, 0),
                    new Rgb24(5, 0, 0),
                    new Rgb24(6, 0, 0)
                };

            var color = IdMapReadStorage.GetColor(new Rgba32(255, 0, 0, 255), textures);
            Assert.Equal(new Rgb24(2, 0, 0), color);

            color = IdMapReadStorage.GetColor(new Rgba32(0, 255, 0, 255), textures);
            Assert.Equal(new Rgb24(3, 0, 0), color);

            color = IdMapReadStorage.GetColor(new Rgba32(0, 0, 255, 255), textures);
            Assert.Equal(new Rgb24(4, 0, 0), color);

            color = IdMapReadStorage.GetColor(new Rgba32(0, 0, 0, 255), textures);
            Assert.Equal(new Rgb24(1, 0, 0), color);

            color = IdMapReadStorage.GetColor(new Rgba32(0, 0, 255, 0), textures);
            Assert.Equal(new Rgb24(6, 0, 0), color);

            color = IdMapReadStorage.GetColor(new Rgba32(0, 0, 255, 128), textures);
            Assert.Equal(new Rgb24(5, 0, 0), color);
        }

        [Fact]
        public async Task LoadImagePart_RvMatNotFound_ThrowsFileNotFoundException()
        {
            var fileSystem = new GameFileSystemMock();
            var library = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>());
            var partitioner = new ImageryPartitionerMock();
            var storage = new IdMapReadStorage(partitioner, fileSystem, "prefix", library, Arma3MapConfigMock.Island512);

            await Assert.ThrowsAsync<FileNotFoundException>(() => storage.LoadImagePart<Rgba32>(1));
        }

        [Fact]
        public async Task LoadImagePart_ImageNotFound_ThrowsFileNotFoundException()
        {
            var fileSystem = new GameFileSystemMock();
            fileSystem.CreateDirectory("prefix\\data\\layers");
            fileSystem.WriteTextFile("prefix\\data\\layers\\P_000-000.rvmat", RvMat);

            var library = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>
            {
                new TerrainMaterialDefinition(new TerrainMaterial("normal1", "color1", new Rgb24(1, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal2", "color2", new Rgb24(2, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal3", "color3", new Rgb24(3, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal4", "color4", new Rgb24(4, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal5", "color5", new Rgb24(5, 0, 0), null),[]),
                new TerrainMaterialDefinition(new TerrainMaterial("normal6", "color6", new Rgb24(6, 0, 0), null),[])
            });
            var partitioner = new ImageryPartitionerMock();
            var storage = new IdMapReadStorage(partitioner, fileSystem, "prefix", library, Arma3MapConfigMock.Island512);

            await Assert.ThrowsAsync<FileNotFoundException>(() => storage.LoadImagePart<Rgba32>(1));
        }

        [Fact]
        public async Task LoadImagePart_InvalidRvMat_ThrowsApplicationException()
        {
            var fileSystem = new GameFileSystemMock();
            fileSystem.CreateDirectory("prefix\\data\\layers");
            fileSystem.WritePngImage("prefix\\data\\layers\\M_000_000_lca.png", new Image<Rgba32>(256, 256));
            fileSystem.WriteTextFile("prefix\\data\\layers\\P_000-000.rvmat", "invalid content");

            var library = new TerrainMaterialLibrary(new List<TerrainMaterialDefinition>());
            var partitioner = new ImageryPartitionerMock();
            var storage = new IdMapReadStorage(partitioner, fileSystem, "prefix", library, Arma3MapConfigMock.Island512);

            await Assert.ThrowsAsync<ApplicationException>(() => storage.LoadImagePart<Rgba32>(1));
        }

    }
}
