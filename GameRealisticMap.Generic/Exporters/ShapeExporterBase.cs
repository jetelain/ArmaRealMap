using System.Text;
using System.Text.Json;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class ShapeExporterBase : IExporter
    {
        public abstract string Name { get; }

        public IEnumerable<ExportFormat> Formats => [ExportFormat.GeoJSON, ExportFormat.ShapeFile];

        protected abstract IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties);

        public async Task ExportGeoJson(IBuildContext context, string targetFile, IDictionary<string, object>? properties)
        {
            var collection = new FeatureCollection(GetPolygons(context, properties).Select(pair => new Feature(pair.Item1.ToGeoJson(p => p), pair.Item2)).ToList());
            using var stream = File.Create(targetFile);
            await JsonSerializer.SerializeAsync(stream, collection);
        }

        public Task ExportShapeFile(IBuildContext context, string targetFile, IDictionary<string, object>? properties)
        {
            var features = new List<NetTopologySuite.Features.Feature>();
            foreach (var feature in GetPolygons(context, properties))
            {
                var attributesTable = new NetTopologySuite.Features.AttributesTable(feature.Item2 ?? Enumerable.Empty<KeyValuePair<string, object>>());
                features.Add(new NetTopologySuite.Features.Feature(feature.Item1.AsPolygon, attributesTable));
            }
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
