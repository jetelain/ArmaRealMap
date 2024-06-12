using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Test.IO.Converters
{
    public class RandomizationOperationJsonConverterTest
    {

        [Fact]
        public void Serialize()
        {
            var result = JsonSerializer.Serialize(new List<IRandomizationOperation>
            {
                new RotateXRandomization(0, 1, new (2, 3, 4)),
                new RotateYRandomization(0, 1, new (2, 3, 4)),
                new RotateZRandomization(0, 1, new (2, 3, 4)),
                new ScaleUniformRandomization(0, 1, new (2, 3, 4)),
                new TranslateRadiusXYRandomization(0, 1),
                new TranslateRadiusXZRandomization(0, 1),
                new TranslateRadiusYZRandomization(0, 1),
                new TranslateXRandomization(0, 1),
                new TranslateYRandomization(0, 1),
                new TranslateZRandomization(0, 1)
            }, new JsonSerializerOptions() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });

            Assert.Equal(@"[
  {
    ""Type"": ""RotateX"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""RotateY"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""RotateZ"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""ScaleUniform"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""TranslateRadiusXY"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateRadiusXZ"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateRadiusYZ"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateX"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateY"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateZ"",
    ""Min"": 0,
    ""Max"": 1
  }
]", result, false, true, true);

        }

        [Fact]

        public void Deserialize()
        {
            var result = JsonSerializer.Deserialize<List<IRandomizationOperation>>(@"[
  {
    ""Type"": ""RotateX"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""RotateY"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""RotateZ"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""ScaleUniform"",
    ""Min"": 0,
    ""Max"": 1,
    ""CenterPoint"": [2,3,4]
  },
  {
    ""Type"": ""TranslateRadiusXY"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateRadiusXZ"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateRadiusYZ"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateX"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateY"",
    ""Min"": 0,
    ""Max"": 1
  },
  {
    ""Type"": ""TranslateZ"",
    ""Min"": 0,
    ""Max"": 1
  }
]", new JsonSerializerOptions() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });

            Assert.NotNull(result);
            Assert.Equal(10, result!.Count);

            var op0 = Assert.IsType<RotateXRandomization>(result[0]);
            Assert.Equal(0, op0.Min);
            Assert.Equal(1, op0.Max);
            Assert.Equal(new Vector3(2, 3, 4), op0.CenterPoint);

            var op1 = Assert.IsType<RotateYRandomization>(result[1]);
            Assert.Equal(0, op1.Min);
            Assert.Equal(1, op1.Max);
            Assert.Equal(new Vector3(2, 3, 4), op1.CenterPoint);

            var op2 = Assert.IsType<RotateZRandomization>(result[2]);
            Assert.Equal(0, op2.Min);
            Assert.Equal(1, op2.Max);
            Assert.Equal(new Vector3(2, 3, 4), op2.CenterPoint);

            var op3 = Assert.IsType<ScaleUniformRandomization>(result[3]);
            Assert.Equal(0, op3.Min);
            Assert.Equal(1, op3.Max);
            Assert.Equal(new Vector3(2, 3, 4), op3.CenterPoint);

            var op4 = Assert.IsType<TranslateRadiusXYRandomization>(result[4]);
            Assert.Equal(0, op4.Min);
            Assert.Equal(1, op4.Max);

            var op5 = Assert.IsType<TranslateRadiusXZRandomization>(result[5]);
            Assert.Equal(0, op5.Min);
            Assert.Equal(1, op5.Max);

            var op6 = Assert.IsType<TranslateRadiusYZRandomization>(result[6]);
            Assert.Equal(0, op6.Min);
            Assert.Equal(1, op6.Max);

            var op7 = Assert.IsType<TranslateXRandomization>(result[7]);
            Assert.Equal(0, op7.Min);
            Assert.Equal(1, op7.Max);

            var op8 = Assert.IsType<TranslateYRandomization>(result[8]);
            Assert.Equal(0, op8.Min);
            Assert.Equal(1, op8.Max);

            var op9 = Assert.IsType< TranslateZRandomization> (result[9]);
            Assert.Equal(0, op9.Min);
            Assert.Equal(1, op9.Max);

        }

    }
}
