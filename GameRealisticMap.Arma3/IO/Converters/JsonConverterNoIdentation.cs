using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.IO.Converters
{
    internal abstract class JsonConverterNoIdentation<T> : JsonConverter<T>
    {
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var innerWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions() { Indented = false }))
            {
                WriteNotIndented(innerWriter, value);
            }
            writer.WriteRawValue(bufferWriter.WrittenSpan, skipInputValidation: true);
        }

        protected abstract void WriteNotIndented(Utf8JsonWriter innerWriter, T value);
    }
}
