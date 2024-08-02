using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature
{
    internal abstract class BasicBuilderBase<T> : PolygonBuilderBase, IDataBuilder<T>
        where T : class, IBasicTerrainData
    {
        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>();
            var railways = context.GetData<RailwaysData>();
            var lakes = context.GetData<ElevationWithLakesData>();
            var buildings = context.GetData<BuildingsData>();

            return buildings.Buildings.Select(b => b.Box.Polygon)
                .Concat(roads.Roads.Where(r => r.RoadType != RoadTypeId.Trail).SelectMany(r => r.ClearPolygons))
                .Concat(railways.Railways.SelectMany(r => r.ClearPolygons))
                .Concat(lakes.Lakes.Select(l => l.TerrainPolygon));
        }

        protected abstract T CreateWrapper(List<TerrainPolygon> polygons);

        public virtual T Build(IBuildContext context, IProgressScope scope)
        {
            return CreateWrapper(GetPolygons(context, Enumerable.Empty<TerrainPolygon>(), scope));
        }
    }
}
