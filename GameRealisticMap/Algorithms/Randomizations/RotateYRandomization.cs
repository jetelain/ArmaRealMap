using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class RotateYRandomization : IRandomizationOperation
    {
        public RotateYRandomization(float min, float max, Vector3 centerPoint)
        {
            Min = min;
            Max = max;
            CenterPoint = centerPoint;
        }

        public float Min { get; }

        public float Max { get; }

        public Vector3 CenterPoint { get; }

        public Matrix4x4 GetMatrix(Random random, Vector3 modelCenter)
        {
            return Matrix4x4.CreateRotationY(MathHelper.ToRadians(RandomHelper.GetBetween(random, Min, Max)), CenterPoint);
        }
    }
}
