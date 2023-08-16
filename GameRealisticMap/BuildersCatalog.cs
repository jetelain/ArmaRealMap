using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Satellite;

namespace GameRealisticMap
{
    public class BuildersCatalog : IBuidersCatalog
    {
        private readonly Dictionary<Type, IBuilderAdapter> builders = new Dictionary<Type, IBuilderAdapter>();

        public BuildersCatalog(IProgressSystem progress, IRoadTypeLibrary<IRoadTypeInfos> library, IRailwayCrossingResolver? crossingResolver = null, bool useFullGeoJson = false)
        {
            Register(new OceanBuilder(progress));
            Register(new CoastlineBuilder(progress));
            Register(new RawSatelliteImageBuilder(progress));
            Register(new RawElevationBuilder(progress));
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
            Register(new OrientedObjectBuilder(progress));
            Register(new RailwaysBuilder(progress, crossingResolver));
            Register(new CitiesBuilder(progress));
            Register(new ElevationBuilder(progress, useFullGeoJson));
            Register(new VineyardBuilder(progress));
            Register(new OrchardBuilder(progress));
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
