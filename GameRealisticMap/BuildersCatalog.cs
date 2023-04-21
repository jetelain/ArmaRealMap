using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
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
            Register(new RawSatelliteImageBuilder(progress));
            Register(new RawElevationBuilder(progress));
            Register(new ElevationBuilder(progress));
            Register(new CategoryAreaBuilder(progress));
            Register(new RoadsBuilder(progress, library));
            Register(new BuildingsBuilder(progress));
            Register(new WatercoursesBuilder(progress));
            Register(new WatercourseRadialBuilder(progress));
            Register(new ForestBuilder(progress));
            Register(new ScrubBuilder(progress));
            Register(new RocksBuilder(progress));
            Register(new ForestRadialBuilder(progress));
            Register(new ScrubRadialBuilder(progress));
            Register(new LakesBuilder(progress));
            Register(new ForestEdgeBuilder(progress));
            Register(new SandSurfacesBuilder(progress));
            Register(new ElevationWithLakesBuilder(progress));
            Register(new MeadowsBuilder(progress));
            Register(new GrassBuilder(progress));
        }

        public void Register<TData>(IDataBuilder<TData> builder)
            where TData : class, ITerrainData
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
