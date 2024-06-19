using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GameRealisticMap;
using GameRealisticMap.Osm;

namespace DatasetsLoader
{
    public static class Datasets
    {
        private static string GetBasePath([CallerFilePath]string thisFilePath = "")
        {
            return Environment.GetEnvironmentVariable("GRM_DATASETS") 
                ?? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFilePath)!, "..", "Datasets"));
        }

        private static readonly string basePath = GetBasePath();

        private static readonly ConcurrentDictionary<string, Task<IOsmDataSource>> sources = new ConcurrentDictionary<string, Task<IOsmDataSource>>();

        public static readonly DatasetMap Chaux = new DatasetMap("chaux", TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", 2.5f, 1024));
        public static readonly DatasetMap Coastline = new DatasetMap("coastline", TerrainAreaUTM.CreateFromCenter("43.805011792296725, -1.4100638139572768", 3.25f, 256));
        public static readonly DatasetMap Cutlines = new DatasetMap("cutlines", TerrainAreaUTM.CreateFromCenter("45.9031, -74.4580", 5, 1024));
        public static readonly DatasetMap Island = new DatasetMap("island", TerrainAreaUTM.CreateFromCenter("46.71532290005527, -2.3412981298116162", 4.5f, 2048));
        public static readonly DatasetMap Northern = new DatasetMap("northern", TerrainAreaUTM.CreateFromCenter("69.09342663102946, 18.25374665258632", 6.25f, 512));
        public static readonly DatasetMap Vineyards = new DatasetMap("vineyards", TerrainAreaUTM.CreateFromCenter("48.052571462724465, 7.217234370654195", 2.5f, 2048));
        public static readonly DatasetMap WindTurbine = new DatasetMap("wind_turbine", TerrainAreaUTM.CreateFromCenter("48.62625229319036, 6.820685612478758", 2.5f, 1024));

        public static readonly List<DatasetMap> Maps = new List<DatasetMap>()
        {
            Chaux,
            Coastline,
            Cutlines,
            Island,
            Northern,
            Vineyards,
            WindTurbine
        };

        public static Task<IOsmDataSource> GetOsmDataSource(DatasetMap ds)
        {
            lock (sources)
            {
                return sources.GetOrAdd(ds.Name, n => Task.Run(() => OsmDataSource.CreateFromFile(Path.Combine(basePath, ds.Name + ".pbf.zst"))));
            }
        }
    }
}
