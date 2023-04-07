using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.WaterWays;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Satellite;

namespace GameRealisticMap
{
    public class BuildersCatalog : IBuidersCatalog
    {
        private readonly Dictionary<Type, object> builders = new Dictionary<Type, object>();
        private readonly List<Func<IBuildContext, ITerrainData>> getters = new List<Func<IBuildContext, ITerrainData>>();

        public BuildersCatalog(IProgressSystem progress, IRoadTypeLibrary library)
        {
            Register<RawSatelliteImageData, RawSatelliteImageBuilder>(new RawSatelliteImageBuilder(progress));
            Register<RawElevationData, RawElevationBuilder>(new RawElevationBuilder(progress));
            Register<ElevationData, ElevationBuilder>(new ElevationBuilder(progress));
            Register<CategoryAreaData, CategoryAreaBuilder>(new CategoryAreaBuilder(progress));
            Register<RoadsData, RoadsBuilder>(new RoadsBuilder(progress, library));
            Register<BuildingsData, BuildingsBuilder>(new BuildingsBuilder(progress));
            Register<WaterWaysData, WaterWaysBuilder>(new WaterWaysBuilder(progress));
            Register<ForestData, ForestBuilder>(new ForestBuilder(progress));
            Register<ScrubData, ScrubBuilder>(new ScrubBuilder(progress));
            Register<RocksData, RocksBuilder>(new RocksBuilder(progress));
            Register<ForestRadialData, ForestRadialBuilder>(new ForestRadialBuilder(progress));
            Register<ScrubRadialData, ScrubRadialBuilder>(new ScrubRadialBuilder(progress));
            Register<LakesData, LakesBuilder>(new LakesBuilder(progress));
        }

        public void Register<TData, TBuidler>(TBuidler builder)
            where TData : class, ITerrainData
            where TBuidler : class, IDataBuilder<TData>
        {
            builders.Add(typeof(TData), builder);
            getters.Add((IBuildContext ctx) => ctx.GetData<TData>());
        }

        public IDataBuilder<TData> Get<TData>() where TData : class, ITerrainData
        {
            if (builders.TryGetValue(typeof(TData), out var builder))
            {
                return (IDataBuilder<TData>)builder;
            }
            throw new NotSupportedException($"No builder for '{typeof(TData).Name}'");
        }

        public List<ITerrainData> GetAll(IBuildContext ctx)
        {
            return getters.Select(g => g(ctx)).ToList();
        }
    }
}
