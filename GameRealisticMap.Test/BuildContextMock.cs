using GameRealisticMap.Osm;
using HugeImages.Storage;

namespace GameRealisticMap.Test
{
    internal class BuildContextMock : IBuildContext
    {
        private readonly Dictionary<Type, object> datas = new Dictionary<Type, object>();

        public BuildContextMock(ITerrainArea area, IOsmDataSource osmSource)
        {
            Area = area;
            OsmSource = osmSource;
        }

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public IImageryOptions Imagery => throw new NotImplementedException();

        public IHugeImageStorage HugeImageStorage => throw new NotImplementedException();

        public T GetData<T>()
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
    }
}
