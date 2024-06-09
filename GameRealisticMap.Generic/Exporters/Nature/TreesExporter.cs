using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Trees;

namespace GameRealisticMap.Generic.Exporters.Nature
{
    internal class TreesExporter : PointExporterBase
    {
        public override string Name => "Trees";

        protected override IEnumerable<(TerrainPoint, IDictionary<string, object>?)> GetPoints(IBuildContext context, IDictionary<string, object>? properties)
        {
            return context.GetData<TreesData>().Points.Select(p => (p, properties));
        }
    }
}
