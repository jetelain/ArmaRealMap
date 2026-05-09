using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Conditions
{
    public class NoneConditionEvaluatorTest
    {
        private readonly NoneConditionEvaluator _evaluator = new NoneConditionEvaluator();

        [Fact]
        public void GetPointContext_ReturnsNonePointConditionContext()
        {
            var ctx = _evaluator.GetPointContext(new TerrainPoint(10, 20));

            Assert.Equal(float.MaxValue, ctx.DistanceToOcean);
            Assert.Equal(float.MaxValue, ctx.DistanceToRoad);
            Assert.Equal(100f, ctx.Elevation);
            Assert.Equal(0f, ctx.Slope);
            Assert.False(ctx.IsOcean);
            Assert.False(ctx.IsResidential);
            Assert.False(ctx.IsCommercial);
            Assert.False(ctx.IsIndustrial);
            Assert.False(ctx.IsRetail);
            Assert.False(ctx.IsMilitary);
            Assert.False(ctx.IsFarmyard);
            Assert.False(ctx.IsUrban);
            Assert.False(ctx.IsRoadMotorway);
            Assert.False(ctx.IsRoadPrimary);
            Assert.False(ctx.IsRoadSecondary);
            Assert.False(ctx.IsRoadSimple);
            Assert.False(ctx.IsRoadPath);
        }

        [Fact]
        public void GetPointContext_WithRoad_ReturnsSameDefaults()
        {
            var ctx = _evaluator.GetPointContext(new TerrainPoint(10, 20), road: null);

            Assert.Equal(float.MaxValue, ctx.DistanceToRoad);
        }

        [Fact]
        public void GetPathContext_ReturnsNonePathConditionContext()
        {
            var path = new TerrainPath(new TerrainPoint(0, 0), new TerrainPoint(100, 0));
            var ctx = _evaluator.GetPathContext(path);

            Assert.Equal(path.Length, ctx.Length, 3f);
            Assert.Equal(100f, ctx.MinElevation);
            Assert.Equal(100f, ctx.MaxElevation);
            Assert.Equal(100f, ctx.AvgElevation);
            Assert.False(ctx.IsResidential);
            Assert.False(ctx.IsCommercial);
            Assert.False(ctx.IsIndustrial);
            Assert.False(ctx.IsRetail);
            Assert.False(ctx.IsMilitary);
            Assert.False(ctx.IsFarmyard);
        }

        [Fact]
        public void GetPolygonContext_ReturnsNonePolygonConditionContext()
        {
            var polygon = TerrainPolygon.FromRectangle(new TerrainPoint(0, 0), new TerrainPoint(10, 10));
            var ctx = _evaluator.GetPolygonContext(polygon);

            Assert.Equal((float)polygon.Area, ctx.Area, 3f);
            Assert.Equal(100f, ctx.MinElevation);
            Assert.Equal(100f, ctx.MaxElevation);
            Assert.Equal(100f, ctx.AvgElevation);
            Assert.False(ctx.IsResidential);
            Assert.False(ctx.IsCommercial);
            Assert.False(ctx.IsIndustrial);
            Assert.False(ctx.IsRetail);
            Assert.False(ctx.IsMilitary);
            Assert.False(ctx.IsFarmyard);
        }
    }
}
