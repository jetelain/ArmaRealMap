using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Lakes
{
    internal class LakesBuilder : BasicBuilderBase<LakesData>
    {
        public LakesBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override LakesData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new LakesData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            var water = tags.GetValue("water");
            if (!string.IsNullOrEmpty(water))
            {
                return water == "lake" || water == "pond";
            }
            return tags.GetValue("natural") == "water";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var embankmentMargin = 2.5f * context.Area.GridCellSize;

            return context.GetData<RoadsData>()
                .Roads.Cast<IWay>().Concat(context.GetData<RailwaysData>().Railways)
                .Where(r => r.SpecialSegment == WaySpecialSegment.Embankment)
                .SelectMany(s => s.Path.ToTerrainPolygon(s.Width + embankmentMargin))
                .ToList();
        }
    }
}
