using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.IO.Converters
{
    internal class RandomizationOperationJsonConverter : JsonConverter<IRandomizationOperation>
    {
        public override IRandomizationOperation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<RandomizationOperationJson>(ref reader, options)?.ToRandomizationOperation();
        }

        public override void Write(Utf8JsonWriter writer, IRandomizationOperation value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, RandomizationOperationJson.From(value), options);
        }
    }
}
