using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.IO.Converters
{
    public sealed class ModelInfoReferenceConverter : JsonConverter<ModelInfo>
    {
        private readonly IModelInfoLibrary library;
        private readonly bool allowUnresolvedModel;

        public ModelInfoReferenceConverter(IModelInfoLibrary library, bool allowUnresolvedModel)
        {
            this.library = library;
            this.allowUnresolvedModel = allowUnresolvedModel;
        }

        public override ModelInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var path = reader.GetString();
            if (!string.IsNullOrEmpty(path))
            {
                if (!library.TryResolveByPath(path, out var model))
                {
                    if (!allowUnresolvedModel)
                    {
                        throw new ApplicationException($"Model '{path}' was not found. Did you have installed and enabled all required mods?");
                    }
                    return new ModelInfo("(UNRESOLVED)" + Path.GetFileNameWithoutExtension(path), path, Vector3.Zero);
                }
                return model;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, ModelInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Path);
        }
    }
}
