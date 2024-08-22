using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.Nature.Forests;
using OsmSharp.Tags;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.Surfaces
{
    internal class GrassBuilder : BasicBuilderBase<GrassData>
    {
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
        public override IEnumerable<IDataDependency> Dependencies => base.Dependencies.Concat([
            new DataDependency<ForestData>()
        ]);

        public override GrassData Build(IBuildContext context, IProgressScope scope)
        {
            return CreateWrapper(GetPolygons(context, 
                    context.GetData<AerowaysData>().Aeroways.Where(a => a.Surface == AerowaySurface.Grass).SelectMany(a => a.ToPolygons())
                , scope));
        }
    }
}
