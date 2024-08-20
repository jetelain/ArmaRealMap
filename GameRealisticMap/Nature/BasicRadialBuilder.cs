using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature
{
    internal abstract class BasicRadialBuilder<TEdge,TSource> : IDataBuilder<TEdge>
        where TEdge : class, IBasicTerrainData
        where TSource : class, IBasicTerrainData
    {
        private readonly float width;

        public BasicRadialBuilder(float width)
        {
            this.width = width;
        }
        protected abstract TEdge CreateWrapper(List<TerrainPolygon> polygons);

        protected virtual IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return context.GetData<BuildingsData>().Buildings.Select(b => b.Box.Polygon)
                .Concat(context.GetData<RoadsData>().Roads.Where(r => r.RoadType != RoadTypeId.Trail).SelectMany(r => r.ClearPolygons))
                .Concat(context.GetData<RailwaysData>().Railways.SelectMany(r => r.ClearPolygons))
                .Concat(context.GetData<ElevationWithLakesData>().Lakes.Select(l => l.TerrainPolygon))
                .Concat(context.GetData<ForestData>().Polygons)
                .Concat(context.GetData<ScrubData>().Polygons)
                .Concat(context.GetData<RocksData>().Polygons)
                .Concat(context.GetData<MeadowsData>().Polygons)
                .Concat(context.GetData<FarmlandsData>().Polygons)
                .Concat(context.GetData<CategoryAreaData>().Areas.SelectMany(a => a.PolyList))
                .Concat(context.GetData<OceanData>().Polygons)
                .Concat(context.GetData<OrchardData>().Polygons)
                .Concat(context.GetData<VineyardData>().Polygons);
        }

        public TEdge Build(IBuildContext context, IProgressScope scope)
        {
            var forest = context.GetData<TSource>();

            var priority = GetPriority(context).ToList();

            var radial = forest.Polygons
                .WithProgress(scope, "Crown")
                .SelectMany(e => e.OuterCrown(width))
                .SelectMany(poly => poly.ClippedBy(context.Area.TerrainBounds))

                .SubstractAll(scope, "Priority", priority)
                .ToList();

#if PARALLEL
            using var step = scope.CreateInteger("Merge (Parallel)", radial.Count);
            var final = TerrainPolygon.MergeAllParallel(radial, step);
#else            
            using var step = scope.CreateInteger("Merge (Plain)", radial.Count);
            var final = TerrainPolygon.MergeAll(radial, step);
#endif
            return CreateWrapper(final);
        }
    }
}
