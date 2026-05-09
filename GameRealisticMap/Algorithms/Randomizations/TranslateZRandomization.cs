using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateZRandomization : IRandomizationOperation
    {
        public TranslateZRandomization(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public Matrix4x4 GetMatrix(Random random, Vector3 modelCenter)
        {
            return Matrix4x4.CreateTranslation(0, 0, RandomHelper.GetBetween(random, Min, Max));
        }
    }
}
