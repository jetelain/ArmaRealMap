using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    public class DataDependency<T> : IDataDependency where T : class
    {
        public Task PreAcquire(IContext context, IProgressScope? parentScope = null)
        {
            return context.GetDataAsync<T>(parentScope);
        }

        public Type Type => typeof(T);
    }
}
