using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateXRandomization : IRandomizationOperation
    {
        public TranslateXRandomization(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public Matrix4x4 GetMatrix(Random random)
        {
            return Matrix4x4.CreateTranslation(MathHelper.ToRadians(RandomHelper.GetBetween(random, Min, Max)), 0, 0);
        }
    }
}
