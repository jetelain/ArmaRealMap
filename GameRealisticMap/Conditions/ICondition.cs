namespace GameRealisticMap.Conditions
{
    public interface ICondition<TContext>
    {
        bool Evaluate(TContext context);

        string OriginalString { get; }
    }
}
