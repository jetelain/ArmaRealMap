using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    public class ConditionVM : ConditionVMBase<PointCondition,IPointConditionContext,TerrainPoint>
    {

        public ConditionVM(PointCondition? condition = null) : base(condition)
        {
        }

        internal override PointCondition Parse(string value)
        {
            return new PointCondition(value);
        }
    }
}
