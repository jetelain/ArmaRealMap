using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Nature;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Water;

namespace GameRealisticMap
{
    public class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, ITerrainData> datas = new Dictionary<Type, ITerrainData>();
        private readonly Dictionary<Type, object> builders = new Dictionary<Type, object>();

        public BuildContext(IProgressSystem progress, ITerrainArea area, IOsmDataSource source)
        {
            this.progress = progress;
            Area = area;
            OsmSource = source;
        }

        public void RegisterAll(IProgressSystem progress, IRoadTypeLibrary library)
        {
            Register<RawElevationData, RawElevationBuilder>(new RawElevationBuilder(progress));
            Register<CategoryAreaData, CategoryAreaBuilder>(new CategoryAreaBuilder(progress));
            Register<RoadsData, RoadsBuilder>(new RoadsBuilder(progress, library));
            Register<BuildingsData, BuildingsBuilder>(new BuildingsBuilder(progress));
            Register<WaterData, WaterBuilder>(new WaterBuilder(progress));
            Register<ForestData, ForestBuilder>(new ForestBuilder(progress));
            Register<ScrubData, ScrubBuilder>(new ScrubBuilder(progress));
            Register<RocksData, RocksBuilder>(new RocksBuilder(progress));
            Register<ForestRadialData, ForestRadialBuilder>(new ForestRadialBuilder(progress));
            Register<ScrubRadialData, ScrubRadialBuilder>(new ScrubRadialBuilder(progress));
        }

        public void Register<TData, TBuidler>(TBuidler builder)
            where TData : class, ITerrainData
            where TBuidler : class, IDataBuilder<TData>
        {
            builders.Add(typeof(TData), builder);
        }

        private IProgressSystem progress;

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public IEnumerable<ITerrainData> ComputedData => datas.Values;

        public T GetData<T>()
             where T : class, ITerrainData
        {
            if (datas.TryGetValue(typeof(T), out var cachedData))
            {
                return (T)cachedData;
            }

            if (builders.TryGetValue(typeof(T), out var builder))
            {
                using (progress.CreateScope(builder.GetType().Name.Replace("Builder","")))
                {
                    var builtData = ((IDataBuilder<T>)builder).Build(this);
                    datas[typeof(T)] = builtData;
                    return builtData;
                }
            }

            throw new NotImplementedException();
        }
    }
}
