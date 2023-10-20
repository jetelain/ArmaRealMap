using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Conditions
{
    public interface IConditionEvaluator
    {
        IPointConditionContext GetPointContext(TerrainPoint point, Road? road = null);

        IPathConditionContext GetPathContext(TerrainPath path);

        IPolygonConditionContext GetPolygonContext(TerrainPolygon polygon);
    }
}