using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.Test.GameEngine;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Test.Edit.Imagery
{
    public class SatMapReadStorageTest
    {
        [Fact]
        public async Task LoadImagePart_ValidFile_ReturnsImage()
        {
            var fileSystem = new GameFileSystemMock();
            fileSystem.CreateDirectory("prefix\\data\\layers");
            fileSystem.WritePngImage("prefix\\data\\layers\\S_000_000_lco.png", new Image<Rgba32>(256, 256));

            var partitioner = new ImageryPartitionerMock();

            var storage = new SatMapReadStorage(partitioner, fileSystem, "prefix");

            var result = await storage.LoadImagePart<Rgba32>(1);

            Assert.NotNull(result);
            Assert.Equal(256, result.Width);
            Assert.Equal(256, result.Height);
        }

        [Fact]
        public async Task LoadImagePart_FileNotFound_ThrowsFileNotFoundException()
        {
            var fileSystem = new GameFileSystemMock();
            var partitioner = new ImageryPartitionerMock();
            var storage = new SatMapReadStorage(partitioner, fileSystem, "prefix");

            await Assert.ThrowsAsync<FileNotFoundException>(() => storage.LoadImagePart<Rgba32>(1));
        }
    }
}
