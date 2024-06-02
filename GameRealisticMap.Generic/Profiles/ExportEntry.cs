using GameRealisticMap.Generic.Exporters;

namespace GameRealisticMap.Generic.Profiles
{
    public class ExportEntry
    {
        public ExportEntry(string exporter, string fileName, ExportFormat format)
        {
            Exporter = exporter;
            FileName = fileName;
            Format = format;
        }

        public string Exporter { get; }

        public string FileName { get; }

        public ExportFormat Format { get; }
    }
}
