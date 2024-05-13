using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class RotateZRandomization : IRandomizationOperation
    {
        public RotateZRandomization(float min, float max, Vector3 centerPoint)
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
            return Matrix4x4.CreateRotationZ(MathHelper.ToRadians(RandomHelper.GetBetween(random, Min, Max)), CenterPoint);
        }
    }
}
