using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Geometries
{
    internal class TerrainPointJsonConverter : JsonConverter<TerrainPoint>
    {
        public override TerrainPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                var x = reader.GetSingle();
                reader.Read();
                var y = reader.GetSingle();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    reader.Read();
                }
                return new TerrainPoint(x, y);
            }
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Backward compatibility
                var dict = JsonSerializer.Deserialize<Dictionary<string, float>>(ref reader, options);
                if (dict == null)
                {
                    return null;
                }
                return new TerrainPoint(dict["x"], dict["y"]);
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, TerrainPoint value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}