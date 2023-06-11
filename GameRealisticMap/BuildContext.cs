using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap
{
    public class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, object> datas = new Dictionary<Type, object>();
        private readonly IProgressSystem progress;
        private readonly IBuidersCatalog catalog;

        public BuildContext(IBuidersCatalog catalog, IProgressSystem progress, ITerrainArea area, IOsmDataSource source, IImageryOptions imagery, IHugeImageStorage? his = null)
        {
            this.progress = progress;
            this.catalog = catalog;
            Area = area;
            OsmSource = source;
            Imagery = imagery;
            HugeImageStorage = his ?? new TemporaryHugeImageStorage();
        }

        public IBuidersCatalog Catalog => catalog;

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public IImageryOptions Imagery { get; }

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
            using (progress.CreateScope(builder.GetType().Name.Replace("Builder","")))
            {
                var builtData = builder.Build(this);
                datas[typeof(T)] = builtData;
                return builtData;
            }
        }
    }
}
