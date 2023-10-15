using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    [JsonConverter(typeof(PolygonConditionJsonConverter))]
    public sealed class PolygonCondition : ICondition<IPolygonConditionContext>
    {
        private readonly string condition;
        private readonly Expression<Func<IPolygonConditionContext, bool>> expression;
        private readonly Lazy<Func<IPolygonConditionContext, bool>> evaluate;

        public PolygonCondition(string condition)
        {
            this.condition = condition;
            this.expression = TagFilterLanguage.Instance.Parse<IPolygonConditionContext>(condition);
            this.evaluate = new(() => expression.Compile(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public bool Evaluate(IPolygonConditionContext evaluator)
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
