using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;

namespace GameRealisticMap.Nature
{
    internal abstract class BasicRadialBuilder<TEdge,TSource> : IDataBuilder<TEdge>
        where TEdge : class, IBasicTerrainData
        where TSource : class, IBasicTerrainData
    {
        private readonly IProgressSystem progress;
        private readonly float width;

        public BasicRadialBuilder(IProgressSystem progress, float width)
        {
            this.progress = progress;
            this.width = width;
        }
        protected abstract TEdge CreateWrapper(List<TerrainPolygon> polygons);

        protected virtual IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>();
            var lakes = context.GetData<LakesData>();
            var buildings = context.GetData<BuildingsData>();
            var forest = context.GetData<ForestData>();
            var scrub = context.GetData<ScrubData>();
            var rocks = context.GetData<RocksData>();
            var meta = context.GetData<CategoryAreaData>();

            return buildings.Buildings.Select(b => b.Box.Polygon)
                .Concat(roads.Roads.Where(r => r.RoadType != RoadTypeId.Trail).SelectMany(r => r.ClearPolygons))
                .Concat(lakes.Polygons)
                .Concat(forest.Polygons)
                .Concat(scrub.Polygons)
                .Concat(rocks.Polygons)
                .Concat(meta.Areas.SelectMany(a => a.PolyList));
        }

        public TEdge Build(IBuildContext context)
        {
            var forest = context.GetData<TSource>();
            var priority = GetPriority(context);

            var radial = forest.Polygons
                .ProgressStep(progress, "Crown")
                .SelectMany(e => e.OuterCrown(width))
                .SelectMany(poly => poly.ClippedBy(context.Area.TerrainBounds))

                .ProgressStep(progress, "Priority")
                .SelectMany(l => l.SubstractAll(priority))
                .ToList();

            using var step = progress.CreateStep("Merge", 1);

            var final = TerrainPolygon.MergeAll(radial);

            return CreateWrapper(final);
        }
    }
}
