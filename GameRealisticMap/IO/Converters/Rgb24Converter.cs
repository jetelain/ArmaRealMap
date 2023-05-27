using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.IO.Converters
{
    public class Rgb24Converter : JsonConverter<Rgb24>
    {
        public override Rgb24 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str= reader.GetString();
            if (!string.IsNullOrEmpty(str))
            {
                return Color.Parse(str).ToPixel<Rgb24>();
            }
            return Color.Transparent;
        }

        public override void Write(Utf8JsonWriter writer, Rgb24 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Color.FromPixel(value).ToHex());
        }
    }
}
