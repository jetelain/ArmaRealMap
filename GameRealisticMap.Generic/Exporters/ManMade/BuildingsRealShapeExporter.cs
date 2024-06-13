using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class BuildingsRealShapeExporter : PolygonExporterBase
    {
        public override string Name => "BuildingsRealShape";

        protected override IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<BuildingsData>().Buildings.SelectMany(l => l.Polygons.Select(p => (p, BuildingsRectangleExporter.GetProperties(l, properties))));
        }
    }
}
