using CoordinateSharp;

namespace ArmaRealMap
{
    internal class AreaInfos
    {
        public MilitaryGridReferenceSystem StartPointMGRS { get; set; }
        public UniversalTransverseMercator StartPointUTM { get; set; }
        public Coordinate SouthWest { get; set; }
        public Coordinate NorthEast { get; set; }
        public Coordinate NorthWest { get; set; }
        public Coordinate SouthEast { get; set; }
        public int CellSize { get; internal set; }
        public int Size { get; internal set; }

        public int Height { get { return Size * CellSize; } }
        public int Width { get { return Size * CellSize; } }
    }
}