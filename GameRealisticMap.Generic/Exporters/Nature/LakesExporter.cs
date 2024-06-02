using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Generic.Exporters.Nature
{
    internal class LakesExporter : ShapeExporterBase
    {
        public override string Name => "Lakes";

        protected override IEnumerable<TerrainPolygon> GetPolygons(IBuildContext context)
        {
            return context.GetData<ElevationWithLakesData>().Lakes.Select(l => l.TerrainPolygon);
        }

        public override List<Feature> ToGeoJson(IBuildContext context)
        {
            return context.GetData<ElevationWithLakesData>().ToGeoJson(p => p).ToList();
        }
    }
}
