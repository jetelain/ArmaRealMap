namespace GameRealisticMap.Conditions
{
    internal class NonePointConditionContext : IPointConditionContext
    {
        public float DistanceToOcean => float.MaxValue;

        public float DistanceToRoad => float.MaxValue;

        public float Elevation => 100;

        public bool IsCommercial => false;

        public bool IsFarmyard => false;

        public bool IsIndustrial => false;

        public bool IsMilitary => false;

        public bool IsOcean => false;

        public bool IsResidential => false;

        public bool IsRetail => false;

        public bool IsRoadMotorway => false;

        public bool IsRoadPath => false;

        public bool IsRoadPrimary => false;

        public bool IsRoadSecondary => false;

        public bool IsRoadSimple => false;

        public bool IsUrban => false;

        public float Slope => 0;
    }
}