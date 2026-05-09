using System.Text.Json;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class PolygonConditionTest
    {
        private void TestCondition(string text, PolygonConditionContextMock whenTrue, PolygonConditionContextMock? whenFalse = null)
        {
            var condition = new PolygonCondition(text);
            Assert.True(condition.Evaluate(whenTrue));
            Assert.False(condition.Evaluate(whenFalse ?? new PolygonConditionContextMock()));
        }

        [Fact]
        public void PolygonCondition_Ctor()
        {
            var condition = new PolygonCondition("Area > 100 && IsResidential");
            Assert.Equal("Area > 100 && IsResidential", condition.OriginalString);
            Assert.Equal("Area > 100 && IsResidential", condition.ToString());
            Assert.Equal("point => ((point.Area > 100) And point.IsResidential)", condition.LambdaString);
        }

        [Fact]
        public void PolygonCondition_Evaluate_Compare()
        {
            TestCondition(nameof(IPolygonConditionContext.Area) + ">10",
                new PolygonConditionContextMock { Area = 15 });

            TestCondition(nameof(IPolygonConditionContext.MinElevation) + ">10",
                new PolygonConditionContextMock { MinElevation = 15 });

            TestCondition(nameof(IPolygonConditionContext.MaxElevation) + ">10",
                new PolygonConditionContextMock { MaxElevation = 15 });

            TestCondition(nameof(IPolygonConditionContext.AvgElevation) + ">10",
                new PolygonConditionContextMock { AvgElevation = 15 });
        }

        [Fact]
        public void PolygonCondition_Evaluate_Tag()
        {
            TestCondition(nameof(IPolygonConditionContext.IsCommercial), new PolygonConditionContextMock { IsCommercial = true });
            TestCondition(nameof(IPolygonConditionContext.IsFarmyard), new PolygonConditionContextMock { IsFarmyard = true });
            TestCondition(nameof(IPolygonConditionContext.IsIndustrial), new PolygonConditionContextMock { IsIndustrial = true });
            TestCondition(nameof(IPolygonConditionContext.IsMilitary), new PolygonConditionContextMock { IsMilitary = true });
            TestCondition(nameof(IPolygonConditionContext.IsResidential), new PolygonConditionContextMock { IsResidential = true });
            TestCondition(nameof(IPolygonConditionContext.IsRetail), new PolygonConditionContextMock { IsRetail = true });
        }

        [Fact]
        public void PolygonCondition_Evaluate_LogicalAnd()
        {
            var condition = new PolygonCondition("IsResidential && Area > 500");
            Assert.True(condition.Evaluate(new PolygonConditionContextMock { IsResidential = true, Area = 1000 }));
            Assert.False(condition.Evaluate(new PolygonConditionContextMock { IsResidential = true, Area = 100 }));
            Assert.False(condition.Evaluate(new PolygonConditionContextMock { IsResidential = false, Area = 1000 }));
        }

        [Fact]
        public void PolygonCondition_Evaluate_LogicalOr()
        {
            var condition = new PolygonCondition("IsResidential || IsMilitary");
            Assert.True(condition.Evaluate(new PolygonConditionContextMock { IsResidential = true }));
            Assert.True(condition.Evaluate(new PolygonConditionContextMock { IsMilitary = true }));
            Assert.False(condition.Evaluate(new PolygonConditionContextMock()));
        }

        [Fact]
        public void PolygonCondition_Evaluate_Negation()
        {
            var condition = new PolygonCondition("!IsResidential");
            Assert.True(condition.Evaluate(new PolygonConditionContextMock { IsResidential = false }));
            Assert.False(condition.Evaluate(new PolygonConditionContextMock { IsResidential = true }));
        }

        [Fact]
        public void PolygonCondition_InvalidExpression_Throws()
        {
            Assert.Throws<TagFilterLanguageException>(() => new PolygonCondition("NoSuchProperty"));
        }
    }

    public class PolygonConditionJsonConverterTest
    {
        [Fact]
        public void PolygonConditionJsonConverter_Serialize()
        {
            Assert.Equal("null", JsonSerializer.Serialize<PolygonCondition?>(null));
            Assert.Equal("\"IsResidential\"", JsonSerializer.Serialize<PolygonCondition?>(new PolygonCondition("IsResidential")));
        }

        [Fact]
        public void PolygonConditionJsonConverter_Deserialize()
        {
            Assert.Null(JsonSerializer.Deserialize<PolygonCondition?>("null"));
            Assert.Null(JsonSerializer.Deserialize<PolygonCondition?>("\"\""));
            Assert.Equal("IsResidential", JsonSerializer.Deserialize<PolygonCondition?>("\"IsResidential\"")?.OriginalString);
        }

        [Fact]
        public void PolygonConditionJsonConverter_RoundTrip()
        {
            var original = new PolygonCondition("Area > 1000 && IsResidential");
            var json = JsonSerializer.Serialize<PolygonCondition?>(original);
            var restored = JsonSerializer.Deserialize<PolygonCondition?>(json);
            Assert.Equal(original.OriginalString, restored?.OriginalString);
        }
    }
}
