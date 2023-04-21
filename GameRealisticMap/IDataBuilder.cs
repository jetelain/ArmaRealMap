namespace GameRealisticMap
{
    public interface IDataBuilder<T> where T : class
    {
        T Build(IBuildContext context);
    }
}
