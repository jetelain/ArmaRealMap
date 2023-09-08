using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    internal class PointConditionJsonConverter : JsonConverter<PointCondition>
    {
        public override PointCondition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!string.IsNullOrEmpty(str))
            {
                return new PointCondition(str);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, PointCondition value, JsonSerializerOptions options)
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
