namespace GameRealisticMap.Generic.Exporters
{
    internal interface IExporter
    {
        string Name { get; }

        Task Export(string filename, ExportFormat format, IBuildContext context);

        IEnumerable<ExportFormat> Formats { get; }
    }
}
