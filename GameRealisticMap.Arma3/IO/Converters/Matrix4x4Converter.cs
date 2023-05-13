using System.Buffers;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.IO.Converters
{
    internal class Matrix4x4Converter : JsonConverterNoIdentation<Matrix4x4>
    {
        public override Matrix4x4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = JsonSerializer.Deserialize<float[]>(ref reader, options);
            if ( str != null)
            {
                if (str.Length == 16)
                {
                    return new Matrix4x4(
                        m11: str[00], m12: str[01], m13: str[02], m14: str[03],
                        m21: str[04], m22: str[05], m23: str[06], m24: str[07],
                        m31: str[08], m32: str[09], m33: str[10], m34: str[11],
                        m41: str[12], m42: str[13], m43: str[14], m44: str[15]);
                }
                if (str.Length == 12)
                {
                    return new Matrix4x4(
                        m11: str[00], m12: str[01], m13: str[02], m14: 0,
                        m21: str[03], m22: str[04], m23: str[05], m24: 0,
                        m31: str[06], m32: str[07], m33: str[08], m34: 0,
                        m41: str[09], m42: str[10], m43: str[11], m44: 1);
                }
                if (str.Length == 3)
                {
                    return new Matrix4x4(
                        m11: 1, m12: 0, m13: 0, m14: 0,
                        m21: 0, m22: 1, m23: 0, m24: 0,
                        m31: 0, m32: 0, m33: 1, m34: 0,
                        m41: str[0], m42: str[1], m43: str[2], m44: 1);
                }
            }
            return Matrix4x4.Identity;
        }

        protected override void WriteNotIndented(Utf8JsonWriter writer, Matrix4x4 value)
        {
            writer.WriteStartArray();

            if (value.M14 == 0 && value.M24 == 0 && value.M34 == 0 && value.M44 == 1)
            {
                if (value.M11 == 1 && value.M12 == 0 && value.M13 == 0
                 && value.M21 == 0 && value.M22 == 1 && value.M23 == 0
                 && value.M31 == 0 && value.M32 == 0 && value.M33 == 1)
                {
                    writer.WriteNumberValue(value.M41);
                    writer.WriteNumberValue(value.M42);
                    writer.WriteNumberValue(value.M43);
                }
                else
                {
                    writer.WriteNumberValue(value.M11);
                    writer.WriteNumberValue(value.M12);
                    writer.WriteNumberValue(value.M13);
                    writer.WriteNumberValue(value.M21);
                    writer.WriteNumberValue(value.M22);
                    writer.WriteNumberValue(value.M23);
                    writer.WriteNumberValue(value.M31);
                    writer.WriteNumberValue(value.M32);
                    writer.WriteNumberValue(value.M33);
                    writer.WriteNumberValue(value.M41);
                    writer.WriteNumberValue(value.M42);
                    writer.WriteNumberValue(value.M43);
                }
            }
            else
            {
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
            }
            writer.WriteEndArray();
        }
    }
}
