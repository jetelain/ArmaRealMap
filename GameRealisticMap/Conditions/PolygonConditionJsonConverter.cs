using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    internal sealed class PolygonConditionJsonConverter : JsonConverter<PolygonCondition>
    {
        public override PolygonCondition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!string.IsNullOrEmpty(str))
            {
                return new PolygonCondition(str);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, PolygonCondition value, JsonSerializerOptions options)
        {
            var str = value.OriginalString;
            if (string.IsNullOrEmpty(str))
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(str);
            }
        }
    }
}