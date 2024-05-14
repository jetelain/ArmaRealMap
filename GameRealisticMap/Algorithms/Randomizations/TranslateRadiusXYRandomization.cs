using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateRadiusXYRandomization : IRandomizationOperation
    {
        public TranslateRadiusXYRandomization(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public Matrix4x4 GetMatrix(Random random)
        {
            return Matrix4x4.CreateTranslation(
                Vector3.Transform(
                    new Vector3(1, 0, 0) * RandomHelper.GetBetween(random, Min, Max),
                    Matrix4x4.CreateRotationZ(RandomHelper.GetBetween(random, -MathF.PI, MathF.PI))));
        }
    }
}
