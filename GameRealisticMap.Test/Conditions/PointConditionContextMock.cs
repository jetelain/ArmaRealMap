using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    internal class PointConditionContextMock : IPointConditionContext
    {
        public float DistanceToOcean { get; set; }

        public float DistanceToRoad { get; set; }

        public float Elevation { get; set; }

        public bool IsCommercial { get; set; }

        public bool IsFarmyard { get; set; }

        public bool IsIndustrial { get; set; }

        public bool IsMilitary { get; set; }

        public bool IsOcean { get; set; }

        public bool IsResidential { get; set; }

        public bool IsRetail { get; set; }

        public bool IsRoadMotorway { get; set; }

        public bool IsRoadPath { get; set; }

        public bool IsRoadPrimary { get; set; }

        public bool IsRoadSecondary { get; set; }

        public bool IsRoadSimple { get; set; }

        public bool IsUrban { get; set; }

        public float Slope { get; set; }
    }
}
