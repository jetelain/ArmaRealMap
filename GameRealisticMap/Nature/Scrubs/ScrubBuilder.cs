using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Scrubs
{
    internal class ScrubBuilder : BasicBuilderBase<ScrubData>
    {
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
