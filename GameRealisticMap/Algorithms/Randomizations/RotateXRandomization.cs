﻿using System.Numerics;

namespace GameRealisticMap.Algorithms.Randomizations
{
    public sealed class RotateXRandomization : IRandomizationOperation
    {
        public RotateXRandomization(float min, float max, Vector3 centerPoint)
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
            return Matrix4x4.CreateRotationX(MathHelper.ToRadians(RandomHelper.GetBetween(random, Min, Max)), CenterPoint);
        }
    }
}
