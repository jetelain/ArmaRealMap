using GameRealisticMap.IO;
using GameRealisticMap.Osm;
using Pmad.HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    /// <summary>
    /// Default implementation of <see cref="IBuildContext"/> that lazily builds and caches data
    /// on demand using the registered <see cref="IBuidersCatalog"/>.
    /// Each data type is built at most once per context instance; subsequent requests return
    /// the cached result. Concurrent build tasks are supported — independent data types may
    /// build in parallel on the thread pool.
    /// </summary>
    public class BuildContext : IBuildContext
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Dictionary<Type, Task> datas = new Dictionary<Type, Task>();
        private readonly IProgressScope rootScope;
        private readonly IBuidersCatalog catalog;
        private readonly IPackageWriter? writer;

        /// <summary>
        /// Initialises a new <see cref="BuildContext"/> with all required inputs.
        /// </summary>
        /// <param name="catalog">The catalog containing all registered <see cref="IDataBuilder{T}"/> instances.</param>
        /// <param name="rootScope">Root progress scope; each builder creates a child scope named after its class.</param>
        /// <param name="area">The geographic terrain area with coordinate conversion utilities.</param>
        /// <param name="source">The OSM data source pre-loaded for the terrain area.</param>
        /// <param name="imagery">Processing options (resolution, road thresholds, satellite settings).</param>
        /// <param name="his">Optional huge-image storage; defaults to a temporary disk-backed store.</param>
        public BuildContext(IBuidersCatalog catalog, IProgressScope rootScope, ITerrainArea area, IOsmDataSource source, IMapProcessingOptions imagery, IHugeImageStorage? his = null, IPackageWriter? writer = null)
        {
            this.rootScope = rootScope;
            this.catalog = catalog;
            this.writer = writer;
            Area = area;
            OsmSource = source;
            Options = imagery;
            HugeImageStorage = his ?? new TemporaryHugeImageStorage();
        }

        /// <summary>The catalog used to resolve and register data builders.</summary>
        public IBuidersCatalog Catalog => catalog;

        /// <inheritdoc/>
        public ITerrainArea Area { get; }

        /// <inheritdoc/>
        public IOsmDataSource OsmSource { get; }

        /// <inheritdoc/>
        public IMapProcessingOptions Options { get; }

        /// <inheritdoc/>
        public IHugeImageStorage HugeImageStorage { get; }

        /// <summary>
        /// Disposes the <see cref="HugeImageStorage"/> if it implements <see cref="IDisposable"/>.
        /// Call this after generation is complete to release temporary disk-mapped image files.
        /// </summary>
        public void DisposeHugeImages()
        {
            (HugeImageStorage as IDisposable)?.Dispose();
        }

        public T GetData<T>(IProgressScope? parentScope = null) 
            where T : class
        {
            return GetDataAsync<T>(parentScope).Result;
        }

        public Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class
        {
            return GetDataTask<T>(parentScope).Unwrap();
        }

        /// <summary>
        /// Returns the <see cref="Task{T}"/> for data type <typeparamref name="T"/>, creating and
        /// registering a new build task if one does not already exist. The outer task resolves
        /// once the cache entry is registered; the inner task resolves when the build completes.
        /// </summary>
        public async Task<Task<T>> GetDataTask<T>(IProgressScope? parentScope = null) where T : class
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                if (datas.TryGetValue(typeof(T), out var data))
                {
                    return (Task<T>)data;
                }
                var newTask = CreateDataTask<T>(parentScope);
                datas.Add(typeof(T), newTask);
                return newTask;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private Task<T> CreateDataTask<T>(IProgressScope? parentScope) where T : class
        {
            var builder = catalog.Get<T>();
            return Task.Run(async () =>
            {
                var name = builder.GetType().Name.Replace("Builder", "");

                using (var scope = (parentScope ?? rootScope).CreateScope(builder.GetType().Name.Replace("Builder", "")))
                {
                    T value;
                    if (builder is IDataBuilderAsync<T> asyncBuilder)
                    {
                        value = await asyncBuilder.BuildAsync(this, scope).ConfigureAwait(false);
                    }
                    else
                    {
                        value = builder.Build(this, scope);
                    }
                    if (writer != null)
                    {
                        var serializer = ContextSerializer.GetSerializer(builder);
                        await serializer.Write(writer, value).ConfigureAwait(false);
                    }
                    return value;
                }
            });
        }

        public IEnumerable<T> GetOfType<T>() where T : class
        {
            return catalog.GetOfType<T>(this);
        }

        /// <summary>
        /// Injects a pre-built value for data type <typeparamref name="T"/>, bypassing the registered builder.
        /// Any subsequent call to <see cref="IContext.GetData{T}"/> will return this value.
        /// Useful in tests and for Studio live-editing where a builder result is supplied externally.
        /// </summary>
        /// <param name="value">The pre-built data value to cache.</param>
        public void SetData<T>(T value)
            where T : class
        {
            datas[typeof(T)] = Task.FromResult(value);
        }
    }
}
