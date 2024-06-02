using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CoordinateSharp.Debuggers;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class ShapeExporterBase : IExporter
    {
        public abstract string Name { get; }

        public IEnumerable<ExportFormat> Formats => [ExportFormat.GeoJSON, ExportFormat.ShapeFile];

        protected abstract IEnumerable<TerrainPolygon> GetPolygons(IBuildContext context);

        public virtual List<Feature> ToGeoJson(IBuildContext context)
        {
            return GetPolygons(context).Select(polygon => new Feature(polygon.ToGeoJson(p => p))).ToList();
        }

        public async Task ExportGeoJson(IBuildContext context, string targetFile)
        {
            var collection = new FeatureCollection(ToGeoJson(context));
            using var stream = File.Create(targetFile);
            await JsonSerializer.SerializeAsync(stream, collection);
        }

        public Task ExportShapeFile(IBuildContext context, string targetFile)
        {
            return Task.CompletedTask;
        }

        public Task Export(string filname, ExportFormat format, IBuildContext context)
        {
            switch (format)
            {
                case ExportFormat.GeoJSON:
                    return ExportGeoJson(context, filname);
                case ExportFormat.ShapeFile:
                    return ExportShapeFile(context, filname);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }
    }
}
