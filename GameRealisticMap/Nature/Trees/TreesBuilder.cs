using GameRealisticMap.Reporting;
using OsmSharp.Geo;

namespace GameRealisticMap.Nature.Trees
{
    internal class TreesBuilder : IDataBuilder<TreesData>
    {
        private readonly IProgressSystem progress;

        public TreesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public TreesData Build(IBuildContext context)
        {
            var points = context.OsmSource.Nodes
                .Where(n => n.Tags != null && n.Tags.GetValue("natural") == "tree")
                .Select(t => context.Area.LatLngToTerrainPoint(t.GetCoordinate()))
                .Where(p => context.Area.IsInside(p))
                .ToList();

            // TODO : Ensure minimal distance with roads and buildings (1m)

            // XXX : Have distinct collection for natural trees ? (outside cities)

            return new TreesData(points);
        }
    }
}
