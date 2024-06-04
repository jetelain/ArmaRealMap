using MapToolkit.DataCells;
using MapToolkit.DataCells.FileFormats;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal abstract class ElevationExporterBase : IExporter
    {
        public abstract string Name { get; }

        protected abstract DemDataCellPixelIsPoint<float> GetDataCell(IBuildContext context);

        public IEnumerable<ExportFormat> Formats => [ExportFormat.EsriAscii, ExportFormat.DemDataCell];

        public Task Export(string filename, ExportFormat format, IBuildContext context, IDictionary<string, object>? properties)
        {
            switch (format)
            {
                case ExportFormat.EsriAscii:
                    return ExportEsriAscii(context, filename);
                case ExportFormat.DemDataCell:
                    return ExportDdc(context, filename);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }

        private Task ExportDdc(IBuildContext context, string filename)
        {
            var cell = GetDataCell(context);
            cell.Save(filename);
            return Task.CompletedTask;
        }

        private Task ExportEsriAscii(IBuildContext context, string filename)
        {
            var cell = GetDataCell(context);
            using var writer = File.CreateText(filename);
            EsriAsciiHelper.SaveDataCell(writer, cell);
            return Task.CompletedTask;
        }
    }
}
