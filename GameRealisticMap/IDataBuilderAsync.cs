using Pmad.ProgressTracking;

namespace GameRealisticMap
{
    internal interface IDataBuilderAsync<T> : IDataBuilder<T> where T : class
    {
        T IDataBuilder<T>.Build(IBuildContext context, IProgressScope scope)
        {
            return BuildAsync(context, scope).Result;
        }

        Task<T> BuildAsync(IBuildContext context, IProgressScope scope);
    }
}