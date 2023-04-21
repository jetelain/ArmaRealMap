using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Surfaces
{
    internal class MeadowsBuilder : BasicBuilderBase<MeadowsData>
    {
        public MeadowsBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override MeadowsData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new MeadowsData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            switch (tags.GetValue("natural"))
            {
                case "meadow":
                case "grassland":
                case "heath": // XXX: Have a specific data ? (could be mapped to a low density scrubs with meadow surface)
                    return true;
            }
            if (tags.GetValue("landuse") == "farmland" && tags.GetValue("crop") == "grass")
            {
                return true;
            }
            return tags.GetValue("landuse") == "meadow";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }
    }
}
