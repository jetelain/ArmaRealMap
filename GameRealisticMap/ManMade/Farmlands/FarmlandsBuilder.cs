using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Farmlands
{
    internal class FarmlandsBuilder : BasicBuilderBase<FarmlandsData>
    {
        public FarmlandsBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override FarmlandsData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new FarmlandsData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("landuse") == "farmland" && tags.GetValue("crop") != "grass";
            // Grass is processed as a Meadow
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }
    }
}
