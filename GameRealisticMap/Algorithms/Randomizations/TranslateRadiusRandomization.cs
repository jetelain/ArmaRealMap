using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class TranslateRadiusRandomization : IRandomizationOperation
    {
        private Func<float, Matrix4x4> rotationFactory;

        public TranslateRadiusRandomization(float min, float max, Vector3 vector)
        {
            Min = min;
            Max = max;
            Vector = vector;

            if (Vector.X == 0)
            {
                rotationFactory = Matrix4x4.CreateRotationX;
            }
            else if (Vector.Y == 0)
            {
                rotationFactory = Matrix4x4.CreateRotationY;
            }
            else if (Vector.Z == 0)
            {
                rotationFactory = Matrix4x4.CreateRotationZ;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public float Min { get; }

        public float Max { get; }

        public Vector3 Vector { get; }

        public Matrix4x4 GetMatrix(Random random)
        {
            return Matrix4x4.CreateTranslation(
                Vector3.Transform(
                    Vector * RandomHelper.GetBetween(random, Min, Max), 
                    rotationFactory(RandomHelper.GetBetween(random, -MathF.PI, MathF.PI))));
        }
    }
}
