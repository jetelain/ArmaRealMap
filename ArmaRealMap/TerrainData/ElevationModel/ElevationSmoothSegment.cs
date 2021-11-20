using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearRegression;

namespace ArmaRealMap.TerrainData.ElevationModel
{
    internal class ElevationSmoothSegment
    {
        private readonly List<Tuple<float, ElevationConstraintNode>> nodes = new List<Tuple<float, ElevationConstraintNode>>();
        private readonly float sampling;

        public ElevationSmoothSegment(ElevationConstraintNode start, float sampling)
        {
            this.sampling = sampling;
            nodes.Add(new Tuple<float, ElevationConstraintNode>(0, start));
        }

        public void Add(float lengthFromStart, ElevationConstraintNode point)
        {
            nodes.Add(new Tuple<float, ElevationConstraintNode>(lengthFromStart, point));
        }

        public void Apply()
        {
            var until = sampling;
            var samples = new List<Tuple<float, ElevationConstraintNode>>();
            foreach (var tuple in nodes)
            {
                samples.Add(tuple);
                if (tuple.Item1 > until)
                {
                    Smooth(samples);
                    until += sampling;
                    samples.Clear();
                    samples.Add(tuple);
                }
            }
            if (samples.Count > 1)
            {
                Smooth(samples);
            }
        }

        private void Smooth(List<Tuple<float, ElevationConstraintNode>> samples)
        {
            var fit = SimpleRegression.Fit(samples.Select(e => new Tuple<double, double>(e.Item1, e.Item2.Elevation.Value)));

            foreach(var sample in samples)
            {
                var initial = sample.Item2.Elevation.Value;
                var smoothed = fit.Item1 + (fit.Item2 * sample.Item1);

                sample.Item2.SetElevation((float)smoothed);
            }

        }
    }
}