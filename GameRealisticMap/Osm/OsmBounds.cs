namespace GameRealisticMap.Osm
{
    internal class OsmBounds
    {
        public OsmBounds(ITerrainArea area)
        {
            var points = area.GetLatLngBounds().ToList();
            Left =  points.Min(p => p.X);
            Right = points.Max(p => p.X);
            Top =    points.Max(p => p.Y);
            Bottom = points.Min(p => p.Y);
        }

        public OsmBounds(double left, double top, double right, double bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public double Left { get; }

        public double Right { get; }

        public double Top { get; }

        public double Bottom { get; }
    }
}
