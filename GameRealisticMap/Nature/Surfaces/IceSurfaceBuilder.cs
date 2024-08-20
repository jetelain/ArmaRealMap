using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Surfaces
{
    internal class IceSurfaceBuilder : BasicBuilderBase<IceSurfaceData>
    {
        protected override IceSurfaceData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new IceSurfaceData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            if (tags.GetValue("natural") == "glacier")
            {
                return true;
            }
            return tags.GetValue("surface") == "ice";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons)
                .Concat(context.GetData<RocksData>().Polygons)
                .Concat(context.GetData<ScrubData>().Polygons)
                .Concat(context.GetData<ScreeData>().Polygons);
        }
    }
}
