using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.IO.Converters
{
    internal class Matrix4x4Converter : JsonConverter<Matrix4x4>
    {
        public override Matrix4x4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = JsonSerializer.Deserialize<float[]>(ref reader, options);
            if ( str != null)
            {
                return new Matrix4x4(
                    m11: str[00], m12: str[01], m13: str[02], m14: str[03],
                    m21: str[04], m22: str[05], m23: str[06], m24: str[07],
                    m31: str[08], m32: str[09], m33: str[10], m34: str[11],
                    m41: str[12], m42: str[13], m43: str[14], m44: str[15]);
            }
            return Matrix4x4.Identity;
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.M11);
            writer.WriteNumberValue(value.M12);
            writer.WriteNumberValue(value.M13);
            writer.WriteNumberValue(value.M14);
            writer.WriteNumberValue(value.M21);
            writer.WriteNumberValue(value.M22);
            writer.WriteNumberValue(value.M23);
            writer.WriteNumberValue(value.M24);
            writer.WriteNumberValue(value.M31);
            writer.WriteNumberValue(value.M32);
            writer.WriteNumberValue(value.M33);
            writer.WriteNumberValue(value.M34);
            writer.WriteNumberValue(value.M41);
            writer.WriteNumberValue(value.M42);
            writer.WriteNumberValue(value.M43);
            writer.WriteNumberValue(value.M44);
            writer.WriteEndArray();
        }
    }
}
