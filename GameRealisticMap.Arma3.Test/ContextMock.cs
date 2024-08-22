using HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Test
{
    internal class ContextMock : Dictionary<Type, object>, IContext, IDisposable
    {
        public MemoryHugeImageStorage HugeImageStorage { get; } = new MemoryHugeImageStorage();

        IHugeImageStorage IContext.HugeImageStorage => HugeImageStorage;

        public void Dispose()
        {
            HugeImageStorage.Dispose();
        }

        public T GetData<T>(IProgressScope? parentScope = null) where T : class
        {
            return (T)this[typeof(T)];
        }

        public IEnumerable<T> GetOfType<T>() where T : class
        {
            foreach (var pair in this)
            {
                if (typeof(T).IsAssignableFrom(pair.Key))
                {
                    yield return (T)pair.Value;
                }
            }
        }

        public void Add<T>(T value) where T : class
        {
            Add(typeof(T), value);
        }

        public Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class
        {
            return Task.FromResult(GetData<T>(parentScope));
        }
    }
}
