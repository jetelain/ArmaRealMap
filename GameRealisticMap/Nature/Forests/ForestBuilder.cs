using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Cutlines;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Forests
{
    internal class ForestBuilder : BasicBuilderBase<ForestData>
    {
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
            var cutlines = context.GetData<CutlinesData>();

            return base.GetPriority(context)
                .Concat(cutlines.Polygons);
        }
    }
}
