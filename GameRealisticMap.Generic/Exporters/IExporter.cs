namespace GameRealisticMap.Generic.Exporters
{
    internal interface IExporter : IExporterInfo
    {


        Task Export(string filename, ExportFormat format, IBuildContext context, IDictionary<string, object>? properties);

    }
}
