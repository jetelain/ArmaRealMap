using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.Nature;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal abstract class DefaultUrbanAreasBuilderBase<TData> : IDataBuilder<TData>
        where TData : class
    {
        private readonly IProgressSystem progress;

        public DefaultUrbanAreasBuilderBase(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public TData Build(IBuildContext context)
        {
            var categories = context.GetData<CategoryAreaData>();

            var areaPolygons = categories.Areas
                .Where(a => a.BuildingType == TragetedType)
                .SelectMany(p => p.PolyList)
                .ToList();

            if (areaPolygons.Count == 0)
            {
                return Create(new List<TerrainPolygon>());
            }

            var polygonsToSubstract = CreateMask(context, categories, areaPolygons);

            var area = Merge(areaPolygons);

            var polygons = area.SubstractAll(progress, "SubstractAll", polygonsToSubstract);

            return Create(polygons.ToList());
        }

        private List<TerrainPolygon> CreateMask(IBuildContext context, CategoryAreaData categories, List<TerrainPolygon> areaPolygons)
        {
            using var report = progress.CreateStep("CreateMask", 1);

            var allData = context.GetOfType<INonDefaultArea>().Where(c => c != categories).ToList();

            var polygonsToSubstract = allData.SelectMany(l => l.Polygons).ToList();
            polygonsToSubstract.AddRange(context.GetData<OrientedObjectData>().Objects.Select(o => TerrainPolygon.FromCircle(o.Point, 1f)));
            polygonsToSubstract.AddRange(context.GetData<TreesData>().Points.Select(o => TerrainPolygon.FromCircle(o, 2f)));
            polygonsToSubstract.AddRange(context.GetData<FencesData>().Fences.SelectMany(o => o.Polygons));
            var priority = CategoryAreaData.Categories.TakeWhile(t => t != TragetedType).ToList();
            if (priority.Count > 0)
            {
                polygonsToSubstract.AddRange(categories.Areas.Where(a => priority.Contains(a.BuildingType)).SelectMany(p => p.PolyList));
            }

            polygonsToSubstract.RemoveAll(p => !areaPolygons.Any(a => a.EnveloppeIntersects(p)));

            return polygonsToSubstract;
        }

        protected abstract BuildingTypeId TragetedType { get; }

        protected abstract TData Create(List<TerrainPolygon> polygons);

        private List<TerrainPolygon> Merge(List<TerrainPolygon> areaPolygons)
        {
            using var report = progress.CreateStep("MergeAll", areaPolygons.Count);
            return TerrainPolygon.MergeAllParallel(areaPolygons, report);
        }
    }
}
