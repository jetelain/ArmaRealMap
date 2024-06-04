using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Generic.Exporters
{
    internal class BasicTerrainExporter<T> : ShapeExporterBase
        where T : class, IBasicTerrainData
    {
        public BasicTerrainExporter(string? name = null)
        {
            Name = name ?? typeof(T).Name.Replace("Data", "");
        }

        protected override IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<T>().Polygons.Select(polygon => (polygon, properties));
        }

        public override string Name { get; }

    }
}
