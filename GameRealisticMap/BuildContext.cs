using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Water;

namespace GameRealisticMap
{
    internal class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, ITerrainData> datas = new Dictionary<Type, ITerrainData>();
        private readonly Dictionary<Type, object> builders = new Dictionary<Type, object>();

        public BuildContext(ITerrainArea area, IOsmDataSource source)
        {
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
        }

        public void Register<TData, TBuidler>(TBuidler builder)
            where TData : class, ITerrainData
            where TBuidler : class, IDataBuilder<TData>
        {
            builders.Add(typeof(TData), builder);
        }

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public T GetData<T>()
             where T : class, ITerrainData
        {
            if (datas.TryGetValue(typeof(T), out var cachedData))
            {
                return (T)cachedData;
            }

            if (builders.TryGetValue(typeof(T), out var builder))
            {
                var builtData = ((IDataBuilder<T>)builder).Build(this);
                datas[typeof(T)] = builtData;
                return builtData;
            }

            throw new NotImplementedException();
        }
    }
}
