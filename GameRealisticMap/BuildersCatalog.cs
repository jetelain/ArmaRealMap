using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Satellite;

namespace GameRealisticMap
{
    public class BuildersCatalog : IBuidersCatalog
    {
        private readonly Dictionary<Type, object> builders = new Dictionary<Type, object>();
        private readonly Dictionary<Type,Func<IBuildContext, object>> getters = new Dictionary<Type, Func<IBuildContext, object>>();

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
            Register(new FencesBuilder(progress));
            Register(new FarmlandsBuilder(progress));
            Register(new TreesBuilder(progress));
        }

        public void Register<TData>(IDataBuilder<TData> builder)
            where TData : class
        {
            builders.Add(typeof(TData), builder);
            getters.Add(typeof(TData), (IBuildContext ctx) => ctx.GetData<TData>());
        }

        public IDataBuilder<TData> Get<TData>() where TData : class
        {
            if (builders.TryGetValue(typeof(TData), out var builder))
            {
                return (IDataBuilder<TData>)builder;
            }
            throw new NotSupportedException($"No builder for '{typeof(TData).Name}'");
        }

        public List<T> GetAll<T>(IBuildContext ctx) where T : class
        {
            return getters
                .Where(p => typeof(T).IsAssignableFrom(p.Key))
                .Select(g => (T)g.Value(ctx))
                .ToList();
        }
    }
}
