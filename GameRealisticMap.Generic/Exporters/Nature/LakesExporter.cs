using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Generic.Exporters.Nature
{
    internal class LakesExporter : ShapeExporterBase
    {
        public override string Name => "Lakes";

        protected override IEnumerable<(TerrainPolygon, IDictionary<string, object>?)> GetPolygons(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<ElevationWithLakesData>().Lakes.Select(l => (l.TerrainPolygon, GetProperties(properties, l)));
        }

        private IDictionary<string, object>? GetProperties(IDictionary<string, object>? properties, LakeWithElevation lake)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_water_elevation"] = lake.WaterElevation;
            dict["grm_border_elevation"] = lake.BorderElevation;
            return dict;
        }
    }
}
