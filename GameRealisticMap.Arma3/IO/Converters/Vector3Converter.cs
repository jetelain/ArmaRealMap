using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.IO.Converters
{
    internal class Vector3Converter : JsonConverterNoIdentation<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<float[]>(ref reader, options);
            if (value == null)
            {
                return Vector3.Zero;
            }
            return new Vector3(value[0], value[1], value[2]);
        }

        protected override void WriteNotIndented(Utf8JsonWriter writer, Vector3 value)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
        }
    }
}