using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class PointConditionTest
    {
        [Fact]
        public void PointCondition_Evaluate_Compare()
        {
            TestCondition(nameof(IPointConditionContext.DistanceToRoad) + ">10", 
                new PointConditionContextMock() { DistanceToRoad = 15 }); 
            
            TestCondition(nameof(IPointConditionContext.DistanceToOcean) + ">10",
                new PointConditionContextMock() { DistanceToOcean = 15 });

            TestCondition(nameof(IPointConditionContext.Elevation) + ">10",
                new PointConditionContextMock() { Elevation = 15 });

            TestCondition(nameof(IPointConditionContext.Slope) + ">10",
                new PointConditionContextMock() { Slope = 15 });
        }

        [Fact]
        public void PointCondition_Evaluate_Tag()
        {
            TestCondition(nameof(IPointConditionContext.IsCommercial), new PointConditionContextMock() { IsCommercial = true });
            TestCondition(nameof(IPointConditionContext.IsFarmyard), new PointConditionContextMock() { IsFarmyard = true });
            TestCondition(nameof(IPointConditionContext.IsIndustrial), new PointConditionContextMock() { IsIndustrial = true });
            TestCondition(nameof(IPointConditionContext.IsMilitary), new PointConditionContextMock() { IsMilitary = true });
            TestCondition(nameof(IPointConditionContext.IsOcean), new PointConditionContextMock() { IsOcean = true });
            TestCondition(nameof(IPointConditionContext.IsResidential), new PointConditionContextMock() { IsResidential = true });
            TestCondition(nameof(IPointConditionContext.IsRetail), new PointConditionContextMock() { IsRetail = true });
            TestCondition(nameof(IPointConditionContext.IsRoadMotorway), new PointConditionContextMock() { IsRoadMotorway = true });
            TestCondition(nameof(IPointConditionContext.IsRoadPath), new PointConditionContextMock() { IsRoadPath = true });
            TestCondition(nameof(IPointConditionContext.IsRoadPrimary), new PointConditionContextMock() { IsRoadPrimary = true });
            TestCondition(nameof(IPointConditionContext.IsRoadSecondary), new PointConditionContextMock() { IsRoadSecondary = true });
            TestCondition(nameof(IPointConditionContext.IsRoadSimple), new PointConditionContextMock() { IsRoadSimple = true });
            TestCondition(nameof(IPointConditionContext.IsUrban), new PointConditionContextMock() { IsUrban = true });
        }

        [Fact]
        public void PointCondition_Ctor()
        {
            var condition = new PointCondition("Elevation > 10 && IsResidential");
            Assert.Equal("Elevation > 10 && IsResidential", condition.ToString());
            Assert.Equal("Elevation > 10 && IsResidential", condition.OriginalString);
            Assert.Equal("point => ((point.Elevation > 10) And point.IsResidential)", condition.LambdaString);
        }

        private void TestCondition(string text, PointConditionContextMock whenTrue, PointConditionContextMock? whenFalse = null)
        {
            var condition = new PointCondition(text);
            Assert.True(condition.Evaluate(whenTrue));
            Assert.False(condition.Evaluate(whenFalse ?? new PointConditionContextMock()));
        }
    }
}
