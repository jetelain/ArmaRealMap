using GameRealisticMap.Osm;
using HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    public class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, object> datas = new Dictionary<Type, object>();
        private readonly IProgressScope rootScope;
        private readonly IBuidersCatalog catalog;

        public BuildContext(IBuidersCatalog catalog, IProgressScope rootScope, ITerrainArea area, IOsmDataSource source, IMapProcessingOptions imagery, IHugeImageStorage? his = null)
        {
            this.rootScope = rootScope;
            this.catalog = catalog;
            Area = area;
            OsmSource = source;
            Options = imagery;
            HugeImageStorage = his ?? new TemporaryHugeImageStorage();
        }

        public IBuidersCatalog Catalog => catalog;

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public IMapProcessingOptions Options { get; }

        public IHugeImageStorage HugeImageStorage { get; }

        public void DisposeHugeImages()
        {
            (HugeImageStorage as IDisposable)?.Dispose();
        }

        public T GetData<T>() 
            where T : class
        {
            if (datas.TryGetValue(typeof(T), out var cachedData))
            {
                return (T)cachedData;
            }

            var builder = catalog.Get<T>();
            using (var scope = rootScope.CreateScope(builder.GetType().Name.Replace("Builder","")))
            {
                var builtData = builder.Build(this, scope);
                datas[typeof(T)] = builtData;
                return builtData;
            }
        }

        public IEnumerable<T> GetOfType<T>() where T : class
        {
            return catalog.GetOfType<T>(this);
        }

        public void SetData<T>(T value)
            where T : class
        {
            datas[typeof(T)] = value;
        }
    }
}
