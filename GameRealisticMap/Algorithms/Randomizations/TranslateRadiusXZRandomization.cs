using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateRadiusXZRandomization : IRandomizationOperation
    {
        public TranslateRadiusXZRandomization(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public Matrix4x4 GetMatrix(Random random, Vector3 modelCenter)
        {
            return Matrix4x4.CreateTranslation(
                Vector3.Transform(
                    new Vector3(1, 0, 0) * RandomHelper.GetBetween(random, Min, Max),
                    Matrix4x4.CreateRotationY(RandomHelper.GetBetween(random, -MathF.PI, MathF.PI))));
        }
    }
}
