using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    [JsonConverter(typeof(PathConditionJsonConverter))]
    public sealed class PathCondition : ICondition<IPathConditionContext>
    {
        private readonly string condition;
        private readonly Expression<Func<IPathConditionContext, bool>> expression;
        private readonly Lazy<Func<IPathConditionContext, bool>> evaluate;

        public PathCondition(string condition)
        {
            this.condition = condition;
            this.expression = TagFilterLanguage.Instance.Parse<IPathConditionContext>(condition);
            this.evaluate = new(() => expression.Compile(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public bool Evaluate(IPathConditionContext evaluator)
        {
            return evaluate.Value(evaluator);
        }

        public string OriginalString => condition;

        public string LambdaString => expression.ToString();

        public override string ToString()
        {
            return condition;
        }
    }
}
