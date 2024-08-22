using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    public interface IDataDependency
    {
        Task PreAcquire(IContext context, IProgressScope? parentScope = null);

        Type Type { get; }
    }
}