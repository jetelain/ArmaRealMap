using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Trees;

namespace GameRealisticMap.Generic.Exporters.Nature
{
    internal class TreeRowsExporter : PathExporterBase
    {
        public override string Name => "TreeRows";

        protected override IEnumerable<(TerrainPath, IDictionary<string, object>?)> GetPaths(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<TreeRowsData>().Rows.Select(row => (row, properties));
        }
    }
}
