using System.Text.Json.Serialization;
using GameRealisticMap.Generic.Exporters;

namespace GameRealisticMap.Generic.Profiles
{
    public class ExportEntry
    {
        public ExportEntry(string exporter, string fileName, ExportFormat? format = null, Dictionary<string, object>? properties = null)
        {
            Exporter = exporter;
            FileName = fileName;
            Format = format;
            Properties = properties;
        }

        public string Exporter { get; }

        public string FileName { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ExportFormat? Format { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Properties { get; }
    }
}
