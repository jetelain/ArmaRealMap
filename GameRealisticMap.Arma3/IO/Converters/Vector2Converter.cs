using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.IO.Converters
{
    internal class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<float[]>(ref reader, options);
            if (value == null)
            {
                return Vector2.Zero;
            }
            return new Vector2(value[0], value[1]);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}