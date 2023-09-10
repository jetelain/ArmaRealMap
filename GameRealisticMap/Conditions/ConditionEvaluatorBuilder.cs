namespace GameRealisticMap.Conditions
{
    internal class ConditionEvaluatorBuilder : IDataBuilder<ConditionEvaluator>
    {
        public ConditionEvaluator Build(IBuildContext context)
        {
            return new ConditionEvaluator(context);
        }
    }
}
