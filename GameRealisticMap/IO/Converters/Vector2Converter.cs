using System.Numerics;
using System.Text.Json;

namespace GameRealisticMap.IO.Converters
{
    public class Vector2Converter : JsonConverterNoIdentation<Vector2>
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

        protected override void WriteNotIndented(Utf8JsonWriter writer, Vector2 value)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}