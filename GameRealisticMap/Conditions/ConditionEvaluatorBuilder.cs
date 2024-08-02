using Pmad.ProgressTracking;

namespace GameRealisticMap.Conditions
{
    internal class ConditionEvaluatorBuilder : IDataBuilder<ConditionEvaluator>
    {
        public ConditionEvaluator Build(IBuildContext context, IProgressScope scope)
        {
            return new ConditionEvaluator(context);
        }
    }
}
