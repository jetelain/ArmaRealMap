using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap
{
    public class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, ITerrainData> datas = new Dictionary<Type, ITerrainData>();
        private readonly IProgressSystem progress;
        private readonly IBuidersCatalog catalog;

        public BuildContext(IBuidersCatalog catalog, IProgressSystem progress, ITerrainArea area, IOsmDataSource source)
        {
            this.progress = progress;
            this.catalog = catalog;
            Area = area;
            OsmSource = source;
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
