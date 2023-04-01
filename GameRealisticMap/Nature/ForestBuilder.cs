using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature
{
    internal class ForestBuilder : BasicBuilderBase<ForestData>
    {
        public ForestBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override ForestData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ForestData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "forest" || tags.GetValue("natural") == "wood";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context);
        }
    }
}
