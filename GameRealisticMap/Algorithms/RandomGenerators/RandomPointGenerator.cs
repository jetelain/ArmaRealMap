using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public static class RandomPointGenerator
    {
        public static IRandomPointGenerator Create(Random random, ITerrainEnvelope envelope, IRandomPointGenerationOptions? options = null, int? defaultNoiseSeed = null)
        {
            var uniform = new UniformRandomPointGenerator(random, envelope.MinPoint, envelope.MaxPoint);
            if (options == null || options.NoiseProportion <= 0d)
            {
                return uniform;
            }

            var noiseBased = new NoiseBasedRandomPointGenerator(uniform, options.NoiseOptions ?? NoiseBasedRandomPointOptions.Default, defaultNoiseSeed);
            if (options.NoiseProportion >= 1d)
            {
                return noiseBased;
            }

            var countNoise = (int)(options.NoiseProportion * 20);
            var countUniform = 20 - countNoise;

            var generators = Enumerable.Repeat<IRandomPointGenerator>(noiseBased, countNoise)
                .Concat(Enumerable.Repeat<IRandomPointGenerator>(uniform, countUniform))
                .OrderBy(x => Random.Shared.Next())
                .ToArray();

            return new MixedRandomPointGenerator(generators);
        }


        public static FastNoiseLite CreateNoise(INoiseOptions options, Random random, int? defaultNoiseSeed)
        {
            var noise = new FastNoiseLite(options.Seed ?? defaultNoiseSeed ?? random.Next());
            noise.SetNoiseType(options.NoiseType);
            noise.SetFrequency(options.Frequency);
            return noise;
        }
    }
}
