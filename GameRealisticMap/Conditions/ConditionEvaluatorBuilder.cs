using GameRealisticMap.IO;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Conditions
{
    internal class ConditionEvaluatorBuilder : IDataBuilder<ConditionEvaluator>, IDataSerializer<ConditionEvaluator>
    {
        public ConditionEvaluator Build(IBuildContext context, IProgressScope scope)
        {
            return new ConditionEvaluator(context);
        }

        public ValueTask<ConditionEvaluator> Read(IPackageReader package, IContext context)
        {
            return ValueTask.FromResult(new ConditionEvaluator(context));
        }

        public Task Write(IPackageWriter package, ConditionEvaluator data)
        {
            return Task.CompletedTask;
        }
    }
}
