using GameRealisticMap.Arma3;

namespace GameRealisticMap.Arma3.Test
{
    public class Arma3ConfigHelperTest
    {
        [Theory]
        [InlineData("ValidName", true)]
        [InlineData("Valid_Name_123", true)]
        [InlineData("ABC", true)]
        [InlineData("", false)]
        [InlineData("invalid-name", false)]
        [InlineData("invalid name", false)]
        [InlineData("invalid.name", false)]
        [InlineData("invalid\\name", false)]
        public void IsValidClassName(string name, bool expected)
        {
            Assert.Equal(expected, Arma3ConfigHelper.IsValidClassName(name));
        }

        [Fact]
        public void ValidateWorldName_ValidName_DoesNotThrow()
        {
            Arma3ConfigHelper.ValidateWorldName("ValidWorldName");
        }

        [Fact]
        public void ValidateWorldName_InvalidName_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => Arma3ConfigHelper.ValidateWorldName("invalid-name"));
            Assert.Contains("invalid-name", ex.Message);
        }

        [Theory]
        [InlineData(@"z\arm\addons\myworld", true)]
        [InlineData(@"valid_prefix\123", true)]
        [InlineData("ValidPrefix", true)]
        [InlineData("invalid prefix", false)]
        [InlineData("invalid-prefix", false)]
        [InlineData("invalid.prefix", false)]
        public void ValidatePboPrefix_ValidAndInvalid(string prefix, bool isValid)
        {
            if (isValid)
            {
                Arma3ConfigHelper.ValidatePboPrefix(prefix);
            }
            else
            {
                Assert.Throws<ArgumentException>(() => Arma3ConfigHelper.ValidatePboPrefix(prefix));
            }
        }

        [Theory]
        [InlineData(1024, 1024, true)]
        [InlineData(2048, 2048, true)]
        [InlineData(512, 256, true)]
        [InlineData(4096, 4096, true)]
        [InlineData(1000, 1000, false)]
        [InlineData(1024, 1000, false)]
        [InlineData(0, 1024, false)]
        [InlineData(3, 3, false)]
        public void IsValidImageSize(int width, int height, bool expected)
        {
            Assert.Equal(expected, Arma3ConfigHelper.IsValidImageSize(width, height));
        }

        [Fact]
        public void ValidClassNameMessage_ContainsName()
        {
            var msg = Arma3ConfigHelper.ValidClassNameMessage("bad-name");
            Assert.Contains("bad-name", msg);
        }
    }
}
