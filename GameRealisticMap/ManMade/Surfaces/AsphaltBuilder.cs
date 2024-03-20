using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Surfaces
{
    internal class AsphaltBuilder : BasicBuilderBase<AsphaltData>
    {
        public AsphaltBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override AsphaltData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new AsphaltData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            var surface = tags.GetValue("surface");
            if (!string.IsNullOrEmpty(surface))
            {
                if (AerowaysBuilder.GetAerowayType(tags) != null ||
                    tags.ContainsKey("highway"))
                {
                    return false;
                }
                return surface == "asphalt";
            }
            return false;
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return context.GetData<ForestData>().Polygons;
        }

        public override AsphaltData Build(IBuildContext context)
        {
            return CreateWrapper(GetPolygons(context,
                    context.GetData<AerowaysData>().Aeroways.Where(a => a.Surface == AerowaySurface.Asphalt).SelectMany(a => a.ToPolygons()).ToList()
                ));
        }
    }
}
