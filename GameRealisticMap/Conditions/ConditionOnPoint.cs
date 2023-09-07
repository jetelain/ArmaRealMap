using System.Linq.Expressions;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    [JsonConverter(typeof(ConditionOnPointJsonConverter))]
    public class ConditionOnPoint
    {
        private readonly string condition;
        private readonly Expression<Func<ConditionOnPointEvaluation, bool>> expression;
        private readonly Func<ConditionOnPointEvaluation, bool> evaluate;

        public ConditionOnPoint(string condition)
        {
            this.condition = condition;

            this.expression = TagFilterLanguage.Instance.Parse<ConditionOnPointEvaluation>(condition);
            this.evaluate = expression.Compile();
        }

        public bool Evaluate (ConditionEvaluator evaluator, TerrainPoint point, Road? road = null)
        {
            return evaluate(new ConditionOnPointEvaluation(evaluator, point, road));
        }

        public string OriginalString => condition;

        public string LambdaString => expression.ToString();


        public override string ToString()
        {
            return condition;
        }
    }
}
