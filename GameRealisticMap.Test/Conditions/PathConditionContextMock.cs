using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    internal class PathConditionContextMock : IPathConditionContext
    {
        public float Length { get; set; }
        public float MinElevation { get; set; }
        public float MaxElevation { get; set; }
        public float AvgElevation { get; set; }
        public bool IsCommercial { get; set; }
        public bool IsFarmyard { get; set; }
        public bool IsIndustrial { get; set; }
        public bool IsMilitary { get; set; }
        public bool IsResidential { get; set; }
        public bool IsRetail { get; set; }
    }
}
