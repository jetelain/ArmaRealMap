using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Conditions
{
    [JsonConverter(typeof(PointConditionJsonConverter))]
    public class PointCondition : ICondition<PointConditionContext>
    {
        private readonly string condition;
        private readonly Expression<Func<PointConditionContext, bool>> expression;
        private readonly Lazy<Func<PointConditionContext, bool>> evaluate;

        public PointCondition(string condition)
        {
            this.condition = condition;
            this.expression = TagFilterLanguage.Instance.Parse<PointConditionContext>(condition);
            this.evaluate = new(() => expression.Compile(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public bool Evaluate (PointConditionContext evaluator)
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
