using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Scrubs;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.RockAreas
{
    internal class ScreeBuilder : BasicBuilderBase<ScreeData>
    {
        protected override ScreeData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new ScreeData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return tags.GetValue("natural") == "scree";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons)
                .Concat(context.GetData<RocksData>().Polygons)
                .Concat(context.GetData<ScrubData>().Polygons);
        }

        public override IEnumerable<IDataDependency> Dependencies => base.Dependencies.Concat([
            new DataDependency<ForestData>(),
            new DataDependency<RocksData>(),
            new DataDependency<ScrubData>()
        ]);

    }
}
