namespace GameRealisticMap.Conditions
{
    public interface ICondition<in TContext>
    {
        bool Evaluate(TContext context);

        string OriginalString { get; }
    }
}
