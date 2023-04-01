using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature
{
    internal class ScrubBuilder : BasicBuilderBase<ScrubData>
    {
        public ScrubBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override ScrubData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ScrubData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("natural") == "scrub";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }
    }
}
