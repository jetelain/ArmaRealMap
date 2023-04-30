using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;
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
            var keepWay =
                context.GetData<RoadsData>().Roads.SelectMany(b => b.Polygons)
                .Concat(context.GetData<BuildingsData>().Buildings.Select(p => p.Box.Polygon));

            var points = context.OsmSource.Nodes
                .Where(n => n.Tags != null && n.Tags.GetValue("natural") == "tree")
                .Select(t => context.Area.LatLngToTerrainPoint(t.GetCoordinate()))
                .ProgressStep(progress, "Trees")
                .Where(p => context.Area.IsInside(p))
                .Select(p => GeometryHelper.KeepAway(p, keepWay, 2))
                .ToList();

            return new TreesData(points);
        }
    }
}
