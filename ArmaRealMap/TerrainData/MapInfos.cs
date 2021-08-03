using System;
using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;
using CoordinateSharp;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    public class MapInfos
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true };

        public MilitaryGridReferenceSystem StartPointMGRS { get; set; }
        public UniversalTransverseMercator StartPointUTM { get; set; }
        public Coordinate SouthWest { get; set; }
        public Coordinate NorthEast { get; set; }
        public Coordinate NorthWest { get; set; }
        public Coordinate SouthEast { get; set; }
        public int CellSize { get; set; }
        public int Size { get; set; }


        public TerrainPoint P1 { get; set; }
        public TerrainPoint P2 { get; set; }

        public int Height => Size * CellSize;
        public int Width => Size * CellSize;

        /// <summary>
        /// meters per pixels 
        /// </summary>
        public double ImageryResolution { get; private set; }
        public int ImageryHeight => (int)(Height / ImageryResolution);
        public int ImageryWidth => (int)(Width / ImageryResolution);

        internal bool IsInside(TerrainPoint p)
        {
            return p.X > P1.X && p.X < P2.X &&
                   p.Y > P1.Y && p.Y < P2.Y;
        }
        internal bool IsInside(NetTopologySuite.Geometries.Coordinate p)
        {
            return p.X > P1.X && p.X < P2.X &&
                   p.Y > P1.Y && p.Y < P2.Y;
        }

        internal static MapInfos Create(Config config)
        {
            var size = config.GridSize;
            var cellSize = config.CellSize;
            var startPointMGRS = new MilitaryGridReferenceSystem(config.BottomLeft.GridZone, config.BottomLeft.D, config.BottomLeft.E, config.BottomLeft.N);
            return Create(startPointMGRS, size, cellSize, config.Resolution);
        }

        internal static MapInfos Create(MilitaryGridReferenceSystem startPointMGRS, int size, int cellSize, double? resolution)
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
                Size = size,
                P1 = new TerrainPoint(0f, 0f),
                P2 = new TerrainPoint(size * cellSize, size * cellSize),
                ImageryResolution = resolution ?? 1d
            };
        }

        public PointF LatLngToPixelsPoint(NetTopologySuite.Geometries.Coordinate n)
        {
            var u = new Coordinate(n.Y, n.X, eagerUTM).UTM;
            return new PointF(
                (float)((u.Easting - StartPointUTM.Easting) / ImageryResolution),
                (float)((Height - (u.Northing - StartPointUTM.Northing)) / ImageryResolution)
            );
        }

        public IEnumerable<PointF> LatLngToPixelsPoints(IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes.Select(LatLngToPixelsPoint);
        }

        public TerrainPoint LatLngToTerrainPoint(NetTopologySuite.Geometries.Coordinate n)
        {
            var u = new Coordinate(n.Y, n.X, eagerUTM).UTM;
            return new TerrainPoint((float)(u.Easting - StartPointUTM.Easting), (float)(u.Northing - StartPointUTM.Northing));
        }

        public IEnumerable<TerrainPoint> LatLngToTerrainPoints(IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes.Select(LatLngToTerrainPoint);
        }

        public PointF TerrainToPixelsPoint(TerrainPoint point)
        {
            return new PointF(
                (float)(point.X / ImageryResolution),
                (float)((Height - point.Y) / ImageryResolution)
            );
        }

        public IEnumerable<PointF> TerrainToPixelsPoints(IEnumerable<TerrainPoint> points)
        {
            return points.Select(TerrainToPixelsPoint);
        }

        public PointF TerrainToPixelsPoint(NetTopologySuite.Geometries.Coordinate point)
        {
            return new PointF(
                (float)(point.X / ImageryResolution),
                (float)((Height - point.Y) / ImageryResolution)
            );
        }

        public IEnumerable<PointF> TerrainToPixelsPoints(IEnumerable<NetTopologySuite.Geometries.Coordinate> points)
        {
            return points.Select(TerrainToPixelsPoint);
        }

        public TerrainPoint LatLngToTerrainPoint(OsmSharp.Node node)
        {
            var coord = new Coordinate(node.Latitude.Value, node.Longitude.Value, eagerUTM).UTM;

            return new TerrainPoint(
                    (float)(coord.Easting - StartPointUTM.Easting),
                    (float)(coord.Northing - StartPointUTM.Northing)
                );
        }
    }
}