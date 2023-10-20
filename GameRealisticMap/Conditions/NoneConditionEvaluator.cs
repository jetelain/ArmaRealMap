using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    public sealed class NoneConditionEvaluator : IConditionEvaluator
    {
        public IPathConditionContext GetPathContext(TerrainPath path)
        {
            return new NonePathConditionContext(path);
        }

        public IPointConditionContext GetPointContext(TerrainPoint point, Road? road = null)
        {
            return new NonePointConditionContext();
        }

        public IPolygonConditionContext GetPolygonContext(TerrainPolygon polygon)
        {
            return new NonePolygonConditionContext(polygon);
        }
    }
}
