using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Forests;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class VineyardBuilder : BasicBuilderBase<VineyardData>
    {
        protected override VineyardData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new VineyardData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "vineyard";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }

        protected override List<TerrainPolygon> MergeIfRequired(List<TerrainPolygon> polygons, IProgressScope scope)
        {
            return polygons; // Do not merge, to be able to place objects at edges and to be able to post-process satellite image
        }
    }
}
