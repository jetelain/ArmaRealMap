using System.Text;
using System.Text.Json;
using GeoJSON.Text.Feature;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class ShapeExporterBase : IExporter
    {
        public abstract string Name { get; }

        public IEnumerable<ExportFormat> Formats => [ExportFormat.GeoJSON, ExportFormat.ShapeFile];

        public abstract FeatureCollection GetGeoJsonFeatureCollection(IBuildContext context, IDictionary<string, object>? properties);

        public abstract List<NetTopologySuite.Features.Feature> GetShapeFeatures(IBuildContext context, IDictionary<string, object>? properties);

        protected static NetTopologySuite.Features.AttributesTable ToAttributesTable(IDictionary<string, object>? data)
        {
            if (data == null)
            {
                return new NetTopologySuite.Features.AttributesTable();
            }
            return new NetTopologySuite.Features.AttributesTable(data.Select(p => new KeyValuePair<string, object>(Truncate(p.Key), p.Value)));
        }

        private static string Truncate(string key)
        {
            if (key.StartsWith("grm_"))
            {
                key = key.Substring(4);
            }
            if (key.Length > 11)
            {
                key = key.Substring(0, 11);
            }
            return key;
        }

        public async Task ExportGeoJson(IBuildContext context, string targetFile, IDictionary<string, object>? properties)
        {
            var collection = GetGeoJsonFeatureCollection(context, properties);
            using var stream = File.Create(targetFile);
            await JsonSerializer.SerializeAsync(stream, collection);
        }

        public Task ExportShapeFile(IBuildContext context, string targetFile, IDictionary<string, object>? properties)
        {
            var features = GetShapeFeatures(context, properties);
            var shapeWriter = new ShapefileDataWriter(targetFile, new GeometryFactory(), Encoding.ASCII)
            {
                Header = GetHeader(features)
            };
            shapeWriter.Write(features);
            return Task.CompletedTask;
        }

        private static DbaseFileHeader GetHeader(List<NetTopologySuite.Features.Feature> features)
        {
            if (features.Count == 0)
            {
                return new DbaseFileHeader(Encoding.ASCII);
            }
            return ShapefileDataWriter.GetHeader(features.First(), features.Count, Encoding.ASCII);
        }

        public Task Export(string filname, ExportFormat format, IBuildContext context, IDictionary<string, object>? properties)
        {
            switch (format)
            {
                case ExportFormat.GeoJSON:
                    return ExportGeoJson(context, filname, properties);

                case ExportFormat.ShapeFile:
                    return ExportShapeFile(context, filname, properties);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }
    }
}
