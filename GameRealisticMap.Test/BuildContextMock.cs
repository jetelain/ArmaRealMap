using GameRealisticMap.Osm;
using HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Test
{
    internal class BuildContextMock : IBuildContext
    {
        private readonly Dictionary<Type, object> datas = new Dictionary<Type, object>();

        public BuildContextMock(ITerrainArea area, IOsmDataSource osmSource, IMapProcessingOptions? options = null)
        {
            Area = area;
            OsmSource = osmSource;
            Options = options ?? new MapProcessingOptions();
        }

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public IMapProcessingOptions Options { get; }

        public IHugeImageStorage HugeImageStorage => throw new NotImplementedException();

        public T GetData<T>(IProgressScope? parentScope = null)
            where T : class
        {
            return (T)datas[typeof(T)];
        }

        public IEnumerable<T> GetOfType<T>() where T : class
        {
            return datas.Values.OfType<T>();
        }

        public void SetData<T>(T value)
            where T : class
        {
            datas[typeof(T)] = value;
        }

        public Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class
        {
            return Task.FromResult(GetData<T>(parentScope));
        }
    }
}
