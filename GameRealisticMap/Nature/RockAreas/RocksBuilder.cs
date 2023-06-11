using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.RockAreas
{
    internal class RocksBuilder : BasicBuilderBase<RocksData>
    {
        public RocksBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override RocksData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new RocksData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("natural") == "bare_rock";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons)
                .Concat(context.GetData<ScrubData>().Polygons);
        }
    }
}
