using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class OrchardBuilder : BasicBuilderBase<OrchardData>
    {
        public OrchardBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override OrchardData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new OrchardData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "orchard";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<VineyardData>().Polygons)
                .Concat(context.GetData<ForestData>().Polygons);
        }

        protected override List<TerrainPolygon> MergeIfRequired(List<TerrainPolygon> polygons)
        {
            return polygons; // Do not merge, to be able to place objects at edges and to be able to post-process satellite image
        }
    }
}
