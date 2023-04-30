using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.ManMade.Roads.Libraries
{
    internal class RoadTypeInfosConverter : JsonConverter<IRoadTypeInfos>
    {
        private readonly IRoadTypeLibrary library;

        public RoadTypeInfosConverter(IRoadTypeLibrary library)
        {
            this.library = library;
        }

        public override IRoadTypeInfos? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var id = reader.GetString();
            if (id == null)
            {
                return null;
            }
            return library.GetInfo(Enum.Parse<RoadTypeId>(id));
        }

        public override void Write(Utf8JsonWriter writer, IRoadTypeInfos value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Id.ToString());
        }
    }
}
