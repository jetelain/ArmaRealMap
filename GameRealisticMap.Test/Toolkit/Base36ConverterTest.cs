using GameRealisticMap.Toolkit;

namespace GameRealisticMap.Test.Toolkit
{
    public class Base36ConverterTest
    {
        [Fact]
        public void ConvertLong()
        {
            Assert.Equal("0", Base36Converter.Convert(0L));
            Assert.Equal("1", Base36Converter.Convert(1L));
            Assert.Equal("9", Base36Converter.Convert(9L));
            Assert.Equal("a", Base36Converter.Convert(10L));
            Assert.Equal("z", Base36Converter.Convert(35L));
            Assert.Equal("10", Base36Converter.Convert(36L));
            Assert.Equal("-1", Base36Converter.Convert(-1L));
            Assert.Equal("-10", Base36Converter.Convert(-36L));
            Assert.Equal("1y2p0ij32e8e7", Base36Converter.Convert(long.MaxValue));
            Assert.Equal("-1y2p0ij32e8e7", Base36Converter.Convert(long.MinValue+1));
            Assert.Equal("-1y2p0ij32e8e8", Base36Converter.Convert(long.MinValue));
        }

        [Fact]
        public void ConvertULong()
        {
            Assert.Equal("0", Base36Converter.Convert(0UL));
            Assert.Equal("1", Base36Converter.Convert(1UL));
            Assert.Equal("9", Base36Converter.Convert(9UL));
            Assert.Equal("a", Base36Converter.Convert(10UL));
            Assert.Equal("z", Base36Converter.Convert(35UL));
            Assert.Equal("10", Base36Converter.Convert(36UL));
            Assert.Equal("3w5e11264sgsf", Base36Converter.Convert(ulong.MaxValue));
        }
    }
}
