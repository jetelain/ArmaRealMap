using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal class ModelInfoReferenceConverter : JsonConverter<ModelInfo>
    {
        private readonly IModelInfoLibrary library;

        public ModelInfoReferenceConverter(IModelInfoLibrary library)
        {
            this.library = library;
        }

        public override ModelInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var path = reader.GetString();
            if (!string.IsNullOrEmpty(path))
            {
                return library.ResolveByPath(path);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, ModelInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Path);
        }
    }
}
