namespace GameRealisticMap.Generic.Exporters
{
    public interface IExporterInfo
    {
        string Name { get; }

        IEnumerable<ExportFormat> Formats { get; }
    }
}