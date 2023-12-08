using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Surfaces
{
    internal class GrassBuilder : BasicBuilderBase<GrassData>
    {
        public GrassBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override GrassData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new GrassData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            var surface = tags.GetValue("surface");
            if (!string.IsNullOrEmpty(surface))
            {
                if (AerowaysBuilder.GetAerowayType(tags) != null)
                {
                    return false;
                }
                return surface == "grass";
            }
            switch (tags.GetValue("landuse"))
            {
                case "grass":
                case "cemetery":
                case "allotments":
                case "village_green":
                    return true;
            }
            switch (tags.GetValue("leisure"))
            {
                case "garden":
                case "park":
                    return true;
            }
            switch (tags.GetValue("natural"))
            {
                case "grass":
                    return true;
            }
            return false;
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context)
                .Concat(context.GetData<ForestData>().Polygons);
        }
        public override GrassData Build(IBuildContext context)
        {
            return CreateWrapper(GetPolygons(context, 
                    context.GetData<AerowaysData>().Aeroways.Where(a => a.Surface == AerowaySurface.Grass).SelectMany(a => a.ToPolygons())
                ));
        }
    }
}
