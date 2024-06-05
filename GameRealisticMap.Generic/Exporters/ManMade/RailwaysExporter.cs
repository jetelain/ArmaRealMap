using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Railways;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class RailwaysExporter : PathExporterBase
    {
        public override string Name => "Railways";

        protected override IEnumerable<(TerrainPath, IDictionary<string, object>?)> GetPaths(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<RailwaysData>().Railways.Select(l => (l.Path, GetProperties(l, properties)));
        }

        internal static IDictionary<string, object>? GetProperties(Railway railway, IDictionary<string, object>? properties)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_segment"] = railway.SpecialSegment.ToString();
            return dict;
        }
    }
}
