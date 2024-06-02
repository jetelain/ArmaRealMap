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

        protected override IEnumerable<TerrainPolygon> GetPolygons(IBuildContext context)
        {
            return context.GetData<T>().Polygons;
        }

        public override List<Feature> ToGeoJson(IBuildContext context)
        {
            return context.GetData<T>().ToGeoJson(p => p).ToList();
        }

        public override string Name { get; }

    }
}
