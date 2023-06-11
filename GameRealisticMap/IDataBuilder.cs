namespace GameRealisticMap
{
    public interface IDataBuilder<out T> where T : class
    {
        T Build(IBuildContext context);
    }
}
