using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Fences;

namespace GameRealisticMap.Generic.Exporters.ManMade
{
    internal class FencesExporter : PathExporterBase
    {
        public override string Name => "Fences";

        protected override IEnumerable<(TerrainPath, IDictionary<string, object>?)> GetPaths(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<FencesData>().Fences.Select(l => (l.Path, GetProperties(l, properties)));
        }

        internal static IDictionary<string, object>? GetProperties(Fence fence, IDictionary<string, object>? properties)
        {
            var dict = properties != null ? new Dictionary<string, object>(properties) : new Dictionary<string, object>();
            dict["grm_fence"] = fence.TypeId.ToString();
            return dict;
        }
    }
}
