using System.Text.Json;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class PathConditionTest
    {
        private void TestCondition(string text, PathConditionContextMock whenTrue, PathConditionContextMock? whenFalse = null)
        {
            var condition = new PathCondition(text);
            Assert.True(condition.Evaluate(whenTrue));
            Assert.False(condition.Evaluate(whenFalse ?? new PathConditionContextMock()));
        }

        [Fact]
        public void PathCondition_Ctor()
        {
            var condition = new PathCondition("Length > 10 && IsResidential");
            Assert.Equal("Length > 10 && IsResidential", condition.OriginalString);
            Assert.Equal("Length > 10 && IsResidential", condition.ToString());
            Assert.Equal("point => ((point.Length > 10) And point.IsResidential)", condition.LambdaString);
        }

        [Fact]
        public void PathCondition_Evaluate_Compare()
        {
            TestCondition(nameof(IPathConditionContext.Length) + ">10",
                new PathConditionContextMock { Length = 15 });

            TestCondition(nameof(IPathConditionContext.MinElevation) + ">10",
                new PathConditionContextMock { MinElevation = 15 });

            TestCondition(nameof(IPathConditionContext.MaxElevation) + ">10",
                new PathConditionContextMock { MaxElevation = 15 });

            TestCondition(nameof(IPathConditionContext.AvgElevation) + ">10",
                new PathConditionContextMock { AvgElevation = 15 });
        }

        [Fact]
        public void PathCondition_Evaluate_Tag()
        {
            TestCondition(nameof(IPathConditionContext.IsCommercial), new PathConditionContextMock { IsCommercial = true });
            TestCondition(nameof(IPathConditionContext.IsFarmyard), new PathConditionContextMock { IsFarmyard = true });
            TestCondition(nameof(IPathConditionContext.IsIndustrial), new PathConditionContextMock { IsIndustrial = true });
            TestCondition(nameof(IPathConditionContext.IsMilitary), new PathConditionContextMock { IsMilitary = true });
            TestCondition(nameof(IPathConditionContext.IsResidential), new PathConditionContextMock { IsResidential = true });
            TestCondition(nameof(IPathConditionContext.IsRetail), new PathConditionContextMock { IsRetail = true });
        }

        [Fact]
        public void PathCondition_Evaluate_LogicalAnd()
        {
            var condition = new PathCondition("IsResidential && Length > 50");
            Assert.True(condition.Evaluate(new PathConditionContextMock { IsResidential = true, Length = 100 }));
            Assert.False(condition.Evaluate(new PathConditionContextMock { IsResidential = true, Length = 10 }));
            Assert.False(condition.Evaluate(new PathConditionContextMock { IsResidential = false, Length = 100 }));
        }

        [Fact]
        public void PathCondition_Evaluate_LogicalOr()
        {
            var condition = new PathCondition("IsResidential || IsMilitary");
            Assert.True(condition.Evaluate(new PathConditionContextMock { IsResidential = true }));
            Assert.True(condition.Evaluate(new PathConditionContextMock { IsMilitary = true }));
            Assert.False(condition.Evaluate(new PathConditionContextMock()));
        }

        [Fact]
        public void PathCondition_Evaluate_Negation()
        {
            var condition = new PathCondition("!IsResidential");
            Assert.True(condition.Evaluate(new PathConditionContextMock { IsResidential = false }));
            Assert.False(condition.Evaluate(new PathConditionContextMock { IsResidential = true }));
        }

        [Fact]
        public void PathCondition_InvalidExpression_Throws()
        {
            Assert.Throws<TagFilterLanguageException>(() => new PathCondition("NoSuchProperty"));
        }
    }

    public class PathConditionJsonConverterTest
    {
        [Fact]
        public void PathConditionJsonConverter_Serialize()
        {
            Assert.Equal("null", JsonSerializer.Serialize<PathCondition?>(null));
            Assert.Equal("\"IsResidential\"", JsonSerializer.Serialize<PathCondition?>(new PathCondition("IsResidential")));
        }

        [Fact]
        public void PathConditionJsonConverter_Deserialize()
        {
            Assert.Null(JsonSerializer.Deserialize<PathCondition?>("null"));
            Assert.Null(JsonSerializer.Deserialize<PathCondition?>("\"\""));
            Assert.Equal("IsResidential", JsonSerializer.Deserialize<PathCondition?>("\"IsResidential\"")?.OriginalString);
        }

        [Fact]
        public void PathConditionJsonConverter_RoundTrip()
        {
            var original = new PathCondition("Length > 100 && IsResidential");
            var json = JsonSerializer.Serialize<PathCondition?>(original);
            var restored = JsonSerializer.Deserialize<PathCondition?>(json);
            Assert.Equal(original.OriginalString, restored?.OriginalString);
        }
    }
}
