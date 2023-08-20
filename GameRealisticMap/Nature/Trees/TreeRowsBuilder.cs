using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature.Trees
{
    public class TreeRowsBuilder : IDataBuilder<TreeRowsData>
    {
        private readonly IProgressSystem progress;

        public TreeRowsBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public TreeRowsData Build(IBuildContext context)
        {
            var nodes = context.OsmSource.All
                .Where(s => s.Tags != null && s.Tags.GetValue("natural") == "tree_row")
                .ToList();

            var rows = new List<TerrainPath>();

            foreach (var way in nodes.ProgressStep(progress, "Paths"))
            {
                foreach (var segment in context.OsmSource.Interpret(way)
                                                .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                                                .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds)))
                {
                    rows.Add(segment);
                }

            }
            return new TreeRowsData(rows);
        }
    }
}
