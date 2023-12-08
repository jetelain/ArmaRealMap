using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public sealed class NoiseBasedRandomPointGenerator : IRandomPointGenerator
    {
        private readonly UniformRandomPointGenerator uniform;
        private readonly int samples;
        private readonly FastNoiseLite noise;
        private readonly float threshold;

        public NoiseBasedRandomPointGenerator(UniformRandomPointGenerator uniform, INoiseBasedRandomPointOptions options, int? defaultNoiseSeed = null)
            : this(uniform, RandomPointGenerator.CreateNoise(options, uniform.Random, defaultNoiseSeed), options.Threshold, options.Samples)
        {

        }

        public NoiseBasedRandomPointGenerator(UniformRandomPointGenerator uniform, FastNoiseLite noise, float threshold = 1.0f, int samples = 10)
        {
            this.threshold = threshold;
            this.samples = samples;
            this.uniform = uniform;
            this.noise = noise;
        }

        public TerrainPoint GetRandomPoint()
        {
            var maxNoise = -10f;
            TerrainPoint? maxPoint = null;
            for (int i = 0; i < samples; ++i)
            {
                var point = uniform.GetRandomPoint();
                var pointNoise = noise.GetNoise(point.X, point.Y);
                if (pointNoise >= threshold)
                {
                    return point;
                }
                if (pointNoise > maxNoise)
                {
                    maxPoint = point;
                    maxNoise = pointNoise;
                }
            }
            return maxPoint!;
        }
    }
}
