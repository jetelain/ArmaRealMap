using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    internal class ConditionOnPointJsonConverter : JsonConverter<ConditionOnPoint>
    {
        public override ConditionOnPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!string.IsNullOrEmpty(str))
            {
                return new ConditionOnPoint(str);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, ConditionOnPoint value, JsonSerializerOptions options)
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
