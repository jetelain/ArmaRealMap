using Pmad.HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    public interface IContext
    {
        T GetData<T>(IProgressScope? parentScope = null) where T : class;

        Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class;

        IEnumerable<T> GetOfType<T>() where T : class;

        IHugeImageStorage HugeImageStorage { get; }
    }
}
