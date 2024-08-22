using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    public interface IDataBuilder<out T> where T : class
    {
        IEnumerable<IDataDependency> Dependencies => Enumerable.Empty<IDataDependency>();

        T Build(IBuildContext context, IProgressScope scope);
    }
}
