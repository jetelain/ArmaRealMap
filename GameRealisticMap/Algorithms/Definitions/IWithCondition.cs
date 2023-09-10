using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IWithCondition<TContext>
    {
        ICondition<TContext>? Condition { get; }
    }
}
