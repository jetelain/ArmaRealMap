using GameRealisticMap.Conditions;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Cutlines;
using GameRealisticMap.ManMade.DefaultUrbanAreas;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Surfaces;
using GameRealisticMap.Nature.DefaultAreas;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Nature.Weather;
using GameRealisticMap.Satellite;

namespace GameRealisticMap
{
    public class BuildersCatalog : IBuidersCatalog
    {
        private readonly Dictionary<Type, IBuilderAdapter> builders = new Dictionary<Type, IBuilderAdapter>();

        public BuildersCatalog(IBuildersConfig config, ISourceLocations sources)
        {
            Register(new OceanBuilder());
            Register(new CoastlineBuilder());
            Register(new RawSatelliteImageBuilder(sources));
            Register(new RawElevationBuilder(sources));
            Register(new CategoryAreaBuilder());
            Register(new RoadsBuilder(config.Roads));
            Register(new BuildingsBuilder(config.Buildings));
            Register(new WatercoursesBuilder());
            Register(new WatercourseRadialBuilder());
            Register(new ForestBuilder());
            Register(new ScrubBuilder());
            Register(new RocksBuilder());
            Register(new ForestRadialBuilder());
            Register(new ScrubRadialBuilder());
            Register(new LakesBuilder());
            Register(new ForestEdgeBuilder());
            Register(new SandSurfacesBuilder());
            Register(new ElevationWithLakesBuilder());
            Register(new MeadowsBuilder());
            Register(new GrassBuilder());
            Register(new FencesBuilder());
            Register(new FarmlandsBuilder());
            Register(new TreesBuilder());
            Register(new OrientedObjectBuilder());
            Register(new RailwaysBuilder(config.RailwayCrossings));
            Register(new CitiesBuilder());
            Register(new ElevationBuilder());
            Register(new VineyardBuilder());
            Register(new OrchardBuilder());
            Register(new TreeRowsBuilder());
            Register(new DefaultAreasBuilder());
            Register(new ProceduralStreetLampsBuilder());
            Register(new SidewalksBuilder());
            Register(new DefaultResidentialAreasBuilder());
            Register(new DefaultCommercialAreasBuilder());
            Register(new DefaultIndustrialAreasBuilder());
            Register(new DefaultMilitaryAreasBuilder());
            Register(new DefaultRetailAreasBuilder());
            Register(new DefaultAgriculturalAreasBuilder());
            Register(new ConditionEvaluatorBuilder());
            Register(new ElevationContourBuilder());
            Register(new WeatherBuilder(sources));
            Register(new IceSurfaceBuilder());
            Register(new ScreeBuilder());
            Register(new ElevationOutOfBoundsBuilder());
            Register(new AirportBuilder());
            Register(new AerowaysBuilder());
            Register(new AsphaltBuilder());
            Register(new CutlinesBuilder());
        }

        public void Register<TData>(IDataBuilder<TData> builder)
            where TData : class
        {
            builders.Add(typeof(TData), new BuilderAdapter<TData>(builder));
        }

        public IDataBuilder<TData> Get<TData>() where TData : class
        {
            if (builders.TryGetValue(typeof(TData), out var builder))
            {
                return (IDataBuilder<TData>)builder.Builder;
            }
            throw new NotSupportedException($"No builder for '{typeof(TData).Name}'");
        }

        public IEnumerable<object> GetAll(IContext ctx)
        {
            return builders
                .Select(g => g.Value.Get(ctx));
        }

        public IEnumerable<T> GetOfType<T>(IContext ctx, Func<Type,bool>? filter = null) where T : class
        {
            return builders
                .Where(p => typeof(T).IsAssignableFrom(p.Key) && (filter == null || filter(p.Key)))
                .Select(g => (T)g.Value.Get(ctx));
        }

        public async Task<IEnumerable<T>> GetOfTypeAsync<T>(IContext ctx, Func<Type, bool>? filter = null) where T : class
        {
            var tasks = builders
                .Where(p => typeof(T).IsAssignableFrom(p.Key) && (filter == null || filter(p.Key)))
                .Select(g => g.Value.GetAsync(ctx))
                .ToArray();
            await Task.WhenAll(tasks);
            return tasks.Select(t => (T)t.Result);
        }

        public int CountOfType<T>(Func<Type, bool>? filter = null) where T : class
        {
            return builders
                .Where(p => typeof(T).IsAssignableFrom(p.Key) && (filter == null || filter(p.Key)))
                .Count();
        }

        public IEnumerable<TResult> VisitAll<TResult>(IDataBuilderVisitor<TResult> visitor)
        {
            return builders
                .Select(g => g.Value.Accept(visitor));
        }
    }
}
