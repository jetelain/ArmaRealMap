using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateYRandomization : IRandomizationOperation
    {
        public TranslateYRandomization(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public Matrix4x4 GetMatrix(Random random, Vector3 modelCenter)
        {
            return Matrix4x4.CreateTranslation(0, MathHelper.ToRadians(RandomHelper.GetBetween(random, Min, Max)), 0);
        }
    }
}
