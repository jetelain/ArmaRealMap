using GameRealisticMap.Toolkit;

namespace GameRealisticMap.Test.Toolkit
{
    public class CaseConverterTest
    {
        [Fact]
        public void ToPascalCase()
        {
            Assert.Equal("AaaBbb", CaseConverter.ToPascalCase("aaa_bbb"));
            Assert.Equal("AaaBbb", CaseConverter.ToPascalCase("AAA_BBB"));
            Assert.Equal("Aaa", CaseConverter.ToPascalCase("AAA"));
            Assert.Equal("AAA", CaseConverter.ToPascalCase("A-A-A"));
            Assert.Equal("Aaa", CaseConverter.ToPascalCase("aaa_"));
            Assert.Equal("A", CaseConverter.ToPascalCase("a"));
            Assert.Equal("", CaseConverter.ToPascalCase("_"));
        }
    }
}
