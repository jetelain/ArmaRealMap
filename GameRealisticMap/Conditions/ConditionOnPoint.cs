using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    public class ConditionOnPoint
    {
        private readonly string condition;

        public ConditionOnPoint(string condition)
        {
            this.condition = condition;
        }

        public bool Evaluate (ConditionEvaluator evaluator, TerrainPoint point, Road? road = null)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return condition;
        }
    }
}
