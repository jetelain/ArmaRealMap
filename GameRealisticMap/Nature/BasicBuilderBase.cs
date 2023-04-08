using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;

namespace GameRealisticMap.Nature
{
    internal abstract class BasicBuilderBase<T> : PolygonBuilderBase, IDataBuilder<T>
        where T : class, IBasicTerrainData
    {
        public BasicBuilderBase(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>();
            var lakes = context.GetData<LakesData>();
            var buildings = context.GetData<BuildingsData>();

            return buildings.Buildings.Select(b => b.Box.Polygon)
                .Concat(roads.Roads.Where(r => r.RoadType != RoadTypeId.Trail).SelectMany(r => r.ClearPolygons))
                .Concat(lakes.Polygons);
        }

        protected abstract T CreateWrapper(List<TerrainPolygon> polygons);

        public virtual T Build(IBuildContext context)
        {
            return CreateWrapper(GetPolygons(context, Enumerable.Empty<TerrainPolygon>()));
        }
    }
}
