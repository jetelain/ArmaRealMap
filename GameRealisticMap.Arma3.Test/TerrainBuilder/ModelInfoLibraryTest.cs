using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.Test.GameEngine;

namespace GameRealisticMap.Arma3.Test.TerrainBuilder
{
    public class ModelInfoLibraryTest
    {
        [Fact]
        public void ReadModelInfoOnly_CorruptedFile()
        {
            var fs = new GameFileSystemMock();
            fs.BinaryFiles.Add("bad.p3d", new MemoryStream());
            var lib = new ModelInfoLibrary(fs);
            var ex = Assert.Throws<ApplicationException>(() => lib.ReadModelInfoOnly("bad.p3d"));
            Assert.Equal("Unable to read file 'bad.p3d': Unable to read beyond the end of the stream.", ex.Message);
        }
    }
}
