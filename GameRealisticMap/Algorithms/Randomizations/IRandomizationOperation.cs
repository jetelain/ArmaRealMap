using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Algorithms.Randomizations
{
    [JsonConverter(typeof(RandomizationOperationJsonConverter))]
    public interface IRandomizationOperation
    {
        Matrix4x4 GetMatrix(Random random, Vector3 modelCenter);
    }
}