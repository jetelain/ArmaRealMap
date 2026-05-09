using OsmSharp.Tags;
using GameRealisticMap.Osm;

namespace GameRealisticMap.Test.Osm
{
    public class TagsCollectionHelperTest
    {
        private static TagsCollection Tags(string key, string value)
            => new TagsCollection { new Tag(key, value) };

        [Theory]
        [InlineData("N", 0f)]
        [InlineData("north", 0f)]
        [InlineData("NNE", 22.5f)]
        [InlineData("NE", 45f)]
        [InlineData("ENE", 67.5f)]
        [InlineData("E", 90f)]
        [InlineData("east", 90f)]
        [InlineData("ESE", 112.5f)]
        [InlineData("SE", 135f)]
        [InlineData("SSE", 157.5f)]
        [InlineData("S", 180f)]
        [InlineData("south", 180f)]
        [InlineData("SSW", 202.5f)]
        [InlineData("SW", 225f)]
        [InlineData("WSW", 247.5f)]
        [InlineData("W", 270f)]
        [InlineData("west", 270f)]
        [InlineData("WNW", 292.5f)]
        [InlineData("NW", 315f)]
        [InlineData("NNW", 337.5f)]
        public void GetDirection_NamedDirections_ReturnsCorrectAngle(string dirValue, float expected)
        {
            var tags = Tags("direction", dirValue);
            Assert.Equal(expected, tags.GetDirection());
        }

        [Fact]
        public void GetDirection_NumericValue_ReturnsParsedAngle()
        {
            var tags = Tags("direction", "123.5");
            Assert.Equal(123.5f, tags.GetDirection());
        }

        [Fact]
        public void GetDirection_NoTag_ReturnsNull()
        {
            var tags = new TagsCollection();
            Assert.Null(tags.GetDirection());
        }

        [Fact]
        public void GetDirection_EmptyValue_ReturnsNull()
        {
            var tags = Tags("direction", "");
            Assert.Null(tags.GetDirection());
        }

        [Fact]
        public void GetDirection_UnknownValue_ReturnsNull()
        {
            var tags = Tags("direction", "diagonal");
            Assert.Null(tags.GetDirection());
        }
    }
}
