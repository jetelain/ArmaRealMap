using System.Diagnostics;

namespace GameRealisticMap.ElevationModel
{
    [DebuggerDisplay("{Min};{Max}")]
    public sealed class ElevationMinMax
    {
        public ElevationMinMax(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }

        public double Max { get; }
    }
}