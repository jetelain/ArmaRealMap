using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class RoadsExporter : PathExporterBase
    {
        public override string Name => "Roads";

        protected override IEnumerable<(TerrainPath, IDictionary<string, object>?)> GetPaths(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<RoadsData>().Roads.Select(l => (l.Path, GetProperties(l, properties)));
        }

        internal static IDictionary<string, object>? GetProperties(Road road, IDictionary<string, object>? properties)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_road"] = road.RoadType.ToString();
            dict["grm_segment"] = road.SpecialSegment.ToString();
            return dict;
        }
    }
}
