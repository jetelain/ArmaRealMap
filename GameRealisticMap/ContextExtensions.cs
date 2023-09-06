namespace GameRealisticMap
{
    public static class ContextExtensions
    {
        public static Lazy<T> GetDataLazy<T>(this IContext context) 
            where T : class
        {
            return new Lazy<T>(() => context.GetData<T>(), LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
