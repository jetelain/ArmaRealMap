using System.Text.Json;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class PointConditionJsonConverterTest
    {

        [Fact]
        public void PointConditionJsonConverter_Serialize()
        {
            Assert.Equal("null", JsonSerializer.Serialize<PointCondition?>(null));
            Assert.Equal("\"IsResidential\"", JsonSerializer.Serialize<PointCondition?>(new PointCondition("IsResidential")));
        }

        [Fact]
        public void PointConditionJsonConverter_Deserialize()
        {
            Assert.Null(JsonSerializer.Deserialize<PointCondition?>("null"));
            Assert.Null(JsonSerializer.Deserialize<PointCondition?>("\"\""));
            Assert.Equal("IsResidential", JsonSerializer.Deserialize<PointCondition?>("\"IsResidential\"")?.OriginalString);
        }
    }
}
