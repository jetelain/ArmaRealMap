using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class TagFilterLanguageExtendedTest
    {
        [Fact]
        public void Parse_ValidPointCondition_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPointConditionContext>("Elevation > 100");
            Assert.Contains("Elevation", expr.ToString());
        }

        [Fact]
        public void Parse_ValidPathCondition_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPathConditionContext>("Length > 50 && IsResidential");
            Assert.Contains("Length", expr.ToString());
            Assert.Contains("IsResidential", expr.ToString());
        }

        [Fact]
        public void Parse_ValidPolygonCondition_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPolygonConditionContext>("Area > 1000");
            Assert.Contains("Area", expr.ToString());
        }

        [Fact]
        public void Parse_OrOperator_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPointConditionContext>("IsResidential || IsMilitary");
            var compiled = expr.Compile();
            Assert.True(compiled(new PointConditionContextMock { IsResidential = true }));
            Assert.True(compiled(new PointConditionContextMock { IsMilitary = true }));
            Assert.False(compiled(new PointConditionContextMock()));
        }

        [Fact]
        public void Parse_AndOperator_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPointConditionContext>("IsResidential && Elevation > 10");
            var compiled = expr.Compile();
            Assert.True(compiled(new PointConditionContextMock { IsResidential = true, Elevation = 15 }));
            Assert.False(compiled(new PointConditionContextMock { IsResidential = true, Elevation = 5 }));
            Assert.False(compiled(new PointConditionContextMock { IsResidential = false, Elevation = 15 }));
        }

        [Fact]
        public void Parse_NegationOperator_ProducesCorrectLambda()
        {
            var expr = TagFilterLanguage.Instance.Parse<IPointConditionContext>("!IsResidential");
            var compiled = expr.Compile();
            Assert.True(compiled(new PointConditionContextMock { IsResidential = false }));
            Assert.False(compiled(new PointConditionContextMock { IsResidential = true }));
        }

        [Fact]
        public void Parse_ComparisonOperators_AllWork()
        {
            Func<IPointConditionContext, bool> Compile(string expr) =>
                TagFilterLanguage.Instance.Parse<IPointConditionContext>(expr).Compile();

            var ctx10 = new PointConditionContextMock { Elevation = 10 };
            var ctx20 = new PointConditionContextMock { Elevation = 20 };

            Assert.True(Compile("Elevation > 15")(ctx20));
            Assert.False(Compile("Elevation > 15")(ctx10));
            Assert.True(Compile("Elevation >= 10")(ctx10));
            Assert.True(Compile("Elevation < 15")(ctx10));
            Assert.False(Compile("Elevation < 15")(ctx20));
            Assert.True(Compile("Elevation <= 10")(ctx10));
            Assert.True(Compile("Elevation == 10")(ctx10));
            Assert.False(Compile("Elevation == 10")(ctx20));
            Assert.True(Compile("Elevation != 10")(ctx20));
            Assert.False(Compile("Elevation != 10")(ctx10));
        }

        [Fact]
        public void Parse_InvalidProperty_ThrowsTagFilterLanguageException()
        {
            var ex = Assert.Throws<TagFilterLanguageException>(
                () => TagFilterLanguage.Instance.Parse<IPointConditionContext>("NoSuchProperty"));

            Assert.Contains("NoSuchProperty", ex.Message);
        }

        [Fact]
        public void Parse_InvalidProperty_ExceptionSegmentMatchesToken()
        {
            var ex = Assert.Throws<TagFilterLanguageException>(
                () => TagFilterLanguage.Instance.Parse<IPointConditionContext>("Elevation > 10 && BadProp"));

            Assert.Equal(18, ex.ErrorSegment.Start);
            Assert.Equal(25, ex.ErrorSegment.End);
        }

        [Fact]
        public void Tokenize_SimpleExpression_ReturnsExpectedTokenCount()
        {
            var tokens = TagFilterLanguage.Instance.Tokenize("IsResidential && Elevation > 10");

            // Tokens: IsResidential, &&, Elevation, >, 10  (whitespace skipped)
            Assert.Equal(5, tokens.Count);
        }

        [Fact]
        public void Tokenize_SingleProperty_ReturnsSingleToken()
        {
            var tokens = TagFilterLanguage.Instance.Tokenize("IsResidential");

            Assert.Single(tokens);
        }
    }
}
