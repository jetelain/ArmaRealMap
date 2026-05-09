using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Test.IO.Converters
{
    public class ModelInfoReferenceConverterTest
    {
        private static readonly ModelInfo KnownModel = new ModelInfo("house", @"\ca\buildings\house.p3d", Vector3.Zero);

        private readonly JsonSerializerOptions options;
        private readonly JsonSerializerOptions optionsStrict;

        public ModelInfoReferenceConverterTest()
        {
            options = new JsonSerializerOptions { Converters = { new ModelInfoReferenceConverter(new LibraryMock(), allowUnresolvedModel: true) } };
            optionsStrict = new JsonSerializerOptions { Converters = { new ModelInfoReferenceConverter(new LibraryMock(), allowUnresolvedModel: false) } };
        }

        private static string ToJson(string? value) => JsonSerializer.Serialize(value);

        [Fact]
        public void Read_KnownPath_ReturnsModelFromLibrary()
        {
            var result = JsonSerializer.Deserialize<ModelInfo>(ToJson(KnownModel.Path), options);
            Assert.NotNull(result);
            Assert.Same(KnownModel, result);
        }

        [Fact]
        public void Read_UnknownPath_AllowUnresolved_ReturnsUnresolvedModel()
        {
            var result = JsonSerializer.Deserialize<ModelInfo>(ToJson(@"\ca\buildings\unknown.p3d"), options);
            Assert.NotNull(result);
            Assert.Equal("(UNRESOLVED)unknown", result!.Name);
            Assert.Equal(@"\ca\buildings\unknown.p3d", result.Path);
        }

        [Fact]
        public void Read_UnknownPath_Strict_ThrowsApplicationException()
        {
            Assert.Throws<ApplicationException>(() => JsonSerializer.Deserialize<ModelInfo>(ToJson(@"\ca\buildings\unknown.p3d"), optionsStrict));
        }

        [Theory]
        [InlineData("null")]
        [InlineData("\"\"")]
        public void Read_NullOrEmptyPath_ReturnsNull(string json)
        {
            var result = JsonSerializer.Deserialize<ModelInfo?>(json, options);
            Assert.Null(result);
        }

        [Fact]
        public void Write_SerializesPath()
        {
            var json = JsonSerializer.Serialize(KnownModel, options);
            var path = JsonSerializer.Deserialize<string>(json);
            Assert.Equal(KnownModel.Path, path);
        }

        private class LibraryMock : IModelInfoLibrary
        {
            public bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model)
            {
                if (path == KnownModel.Path)
                {
                    model = KnownModel;
                    return true;
                }
                model = null;
                return false;
            }

            public bool? IsSlopeLandContact(string path) => null;
            public ModelInfo ResolveByName(string name) => throw new NotImplementedException();
            public ModelInfo ResolveByPath(string path) => throw new NotImplementedException();
            public string? TryGetNoLandContact(string path) => null;
            public bool TryRegister(string name, string path) => throw new NotImplementedException();
            public bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model) => throw new NotImplementedException();
        }
    }
}
