using System.Numerics;
using System.Text;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.Test.GameEngine;

namespace GameRealisticMap.Arma3.Test.TerrainBuilder
{
    internal class GameFileSystemMockWithFindAll : GameFileSystemMock
    {
        public List<string> FindAllResults { get; } = new List<string>();

        public override IEnumerable<string> FindAll(string pattern) => FindAllResults;
    }

    public class ModelInfoLibraryTest
    {
        private static async Task<ModelInfoLibrary> LoadFromJson(string json)
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await lib.Load(stream);
            return lib;
        }

        [Fact]
        public void ReadModelInfoOnly_CorruptedFile()
        {
            var fs = new GameFileSystemMock();
            fs.BinaryFiles.Add("bad.p3d", new MemoryStream());
            var lib = new ModelInfoLibrary(fs);
            var ex = Assert.Throws<ApplicationException>(() => lib.ReadModelInfoOnly("bad.p3d"));
            Assert.Equal("Unable to read file 'bad.p3d': Unable to read beyond the end of the stream.", ex.Message);
        }

        [Fact]
        public void ReadModelInfoOnly_FileNotFound_ReturnsNull()
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            Assert.Null(lib.ReadModelInfoOnly("missing.p3d"));
        }

        [Fact]
        public async Task Load_PopulatesIndexByName()
        {
            var lib = await LoadFromJson("""[{"Name":"house","Path":"\\ca\\buildings\\house.p3d","BoundingCenter":[0,0,0]}]""");
            Assert.True(lib.TryResolveByName("house", out var model));
            Assert.Equal("house", model!.Name);
            Assert.Equal(@"\ca\buildings\house.p3d", model.Path);
        }

        [Fact]
        public async Task Load_IsCaseInsensitive()
        {
            var lib = await LoadFromJson("""[{"Name":"House","Path":"\\ca\\buildings\\house.p3d","BoundingCenter":[0,0,0]}]""");
            Assert.True(lib.TryResolveByName("house", out _));
            Assert.True(lib.TryResolveByName("HOUSE", out _));
        }

        [Fact]
        public async Task Save_ThenLoad_RoundTrips()
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            using var saveStream = new MemoryStream(Encoding.UTF8.GetBytes("""[{"Name":"tree","Path":"\\ca\\plants\\tree.p3d","BoundingCenter":[1,2,3]}]"""));
            await lib.Load(saveStream);

            using var outStream = new MemoryStream();
            await lib.Save(outStream);

            outStream.Position = 0;
            var lib2 = new ModelInfoLibrary(fs);
            await lib2.Load(outStream);

            Assert.True(lib2.TryResolveByName("tree", out var model));
            Assert.Equal(@"\ca\plants\tree.p3d", model!.Path);
            Assert.Equal(new Vector3(1, 2, 3), model.BoundingCenter);
        }

        [Fact]
        public async Task TryResolveByName_UnknownName_ReturnsFalse()
        {
            var lib = await LoadFromJson("[]");
            Assert.False(lib.TryResolveByName("ghost", out var model));
            Assert.Null(model);
        }

        [Fact]
        public void ResolveByName_UnknownName_ThrowsApplicationException()
        {
            var fs = new GameFileSystemMockWithFindAll();
            var lib = new ModelInfoLibrary(fs);
            var ex = Assert.Throws<ApplicationException>(() => lib.ResolveByName("ghost"));
            Assert.Contains("ghost", ex.Message);
        }

        [Fact]
        public void ResolveByPath_FileNotFound_ThrowsApplicationException()
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            var ex = Assert.Throws<ApplicationException>(() => lib.ResolveByPath(@"\ca\buildings\missing.p3d"));
            Assert.Contains("missing.p3d", ex.Message);
        }

        [Fact]
        public async Task TryResolveByPath_AlreadyLoaded_ReturnsSameInstance()
        {
            var lib = await LoadFromJson("""[{"Name":"house","Path":"\\ca\\buildings\\house.p3d","BoundingCenter":[0,0,0]}]""");
            Assert.True(lib.TryResolveByPath(@"\ca\buildings\house.p3d", out var model));
            Assert.Equal("house", model!.Name);
        }

        [Fact]
        public async Task TryResolveByPath_IsCaseInsensitive()
        {
            var lib = await LoadFromJson("""[{"Name":"house","Path":"\\ca\\buildings\\house.p3d","BoundingCenter":[0,0,0]}]""");
            Assert.True(lib.TryResolveByPath(@"\CA\BUILDINGS\HOUSE.P3D", out _));
        }

        [Fact]
        public void TryGetNoLandContact_NoVariant_ReturnsNull()
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            Assert.Null(lib.TryGetNoLandContact(@"\ca\buildings\house.p3d"));
        }

        [Fact]
        public void Models_InitiallyEmpty()
        {
            var fs = new GameFileSystemMock();
            var lib = new ModelInfoLibrary(fs);
            Assert.Empty(lib.Models);
        }

        [Fact]
        public async Task Models_ReflectsLoadedEntries()
        {
            var lib = await LoadFromJson("""
                [
                    {"Name":"house","Path":"\\ca\\buildings\\house.p3d","BoundingCenter":[0,0,0]},
                    {"Name":"tree","Path":"\\ca\\plants\\tree.p3d","BoundingCenter":[0,0,0]}
                ]
                """);
            Assert.Equal(2, lib.Models.Count());
        }
    }
}
