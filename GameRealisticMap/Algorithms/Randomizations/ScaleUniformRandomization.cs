using System.Numerics;
using GameRealisticMap.IO.Converters;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class ScaleUniformRandomization : IRandomizationOperation
    {
        public ScaleUniformRandomization(float min, float max, Vector3 centerPoint)
        {
            Min = min;
            Max = max;
            CenterPoint = centerPoint;
        }

        public float Min { get; }

        public float Max { get; }

        public Vector3 CenterPoint { get; }

        public Matrix4x4 GetMatrix(Random random)
        {
            return Matrix4x4.CreateScale(
                RandomHelper.GetBetween(random, Min, Max),
                RandomHelper.GetBetween(random, Min, Max),
                RandomHelper.GetBetween(random, Min, Max),
                CenterPoint);
        }
    }
}
