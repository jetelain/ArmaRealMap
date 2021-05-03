using System;
using CoordinateSharp;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    internal class MapInfos
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

        internal bool IsInside(PointF p)
        {
            return p.X > StartPointUTM.Easting && p.X < StartPointUTM.Easting + Width &&
                p.Y > StartPointUTM.Northing && p.Y < StartPointUTM.Northing + Height;
        }

        internal static MapInfos Create(Config config)
        {
            var size = config.GridSize;
            var cellSize = config.CellSize;
            var startPointMGRS = new MilitaryGridReferenceSystem(config.BottomLeft.GridZone, config.BottomLeft.D, config.BottomLeft.E, config.BottomLeft.N);
            return Create(startPointMGRS, size, cellSize);
        }

        internal static MapInfos Create(MilitaryGridReferenceSystem startPointMGRS, int size, int cellSize)
        {
            var southWest = MilitaryGridReferenceSystem.MGRStoLatLong(startPointMGRS);

            var startPointUTM = new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting),
                Math.Round(southWest.UTM.Northing));

            var southEast = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting) + (size * cellSize),
                Math.Round(southWest.UTM.Northing)));

            var northEast = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting) + (size * cellSize),
                Math.Round(southWest.UTM.Northing) + (size * cellSize)));

            var northWest = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting),
                Math.Round(southWest.UTM.Northing) + (size * cellSize)));

            return new MapInfos
            {
                StartPointMGRS = startPointMGRS,
                StartPointUTM = startPointUTM,
                SouthWest = southWest,
                NorthEast = northEast,
                NorthWest = northWest,
                SouthEast = southEast,
                CellSize = cellSize,
                Size = size
            };
        }
    }
}