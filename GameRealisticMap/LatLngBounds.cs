using GeoAPI.Geometries;

namespace GameRealisticMap
{
    public sealed class LatLngBounds
    {
        public LatLngBounds(ITerrainArea area)
            : this(area.TerrainBounds.Shell.Select(area.TerrainPointToLatLng).ToList())
        { 
        
        }

        public LatLngBounds(List<Coordinate> points)
        {
            Left =  points.Min(p => p.X);
            Right = points.Max(p => p.X);
            Top =    points.Max(p => p.Y);
            Bottom = points.Min(p => p.Y);
        }

        public LatLngBounds(double left, double top, double right, double bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        /// <summary>
        /// Min X / Min longitude
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// Max X / Max longitude
        /// </summary>
        public double Right { get; }

        /// <summary>
        /// Max Y / Max latitude
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Min Y / Min latitude
        /// </summary>
        public double Bottom { get; }

        public string Name => FormattableString.Invariant($"{Left}_{Bottom}_{Right}_{Top}");

        public override string ToString()
        {
            return Name;
        }
    }
}
