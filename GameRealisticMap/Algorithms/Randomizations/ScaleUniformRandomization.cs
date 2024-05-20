using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class ScaleUniformRandomization : IRandomizationOperation
    {
        public ScaleUniformRandomization(float min, float max, Vector3? centerPoint)
        {
            Min = min;
            Max = max;
            CenterPoint = centerPoint;
        }

        public float Min { get; }

        public float Max { get; }

        public Vector3? CenterPoint { get; }

        public Matrix4x4 GetMatrix(Random random, Vector3 modelCenter)
        {
            var scale = RandomHelper.GetBetween(random, Min, Max);
            return Matrix4x4.CreateScale(scale, scale, scale, CenterPoint ?? modelCenter);
        }
    }
}
