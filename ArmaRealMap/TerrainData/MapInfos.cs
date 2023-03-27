using System;
using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Geometries;
using CoordinateSharp;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    public class MapInfos
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true, Extensions = new EagerLoad_Extensions() { MGRS = false } };

        private static readonly EagerLoad eagerNONE = new EagerLoad(false);

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

        public TerrainPolygon Polygon { get; private set; }

        internal bool IsInside(TerrainPoint p)
        {
            return p.X > P1.X && p.X < P2.X &&
                   p.Y > P1.Y && p.Y < P2.Y;
        }
        internal bool IsInside(GeoAPI.Geometries.Coordinate p)
        {
            return p.X > P1.X && p.X < P2.X &&
                   p.Y > P1.Y && p.Y < P2.Y;
        }

        internal static MapInfos Create(MapConfig config)
        {
            var size = config.GridSize;
            var cellSize = config.CellSize;
            var startPointMGRS = MilitaryGridReferenceSystem.Parse(config.BottomLeft);
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

            int widthInMeters = (size * cellSize);
            var southEast = TerrainToLatLong(startPointUTM, widthInMeters, 0, null);
            var northEast = TerrainToLatLong(startPointUTM, widthInMeters, widthInMeters, null);
            var northWest = TerrainToLatLong(startPointUTM, 0, widthInMeters, null);
            var p1 = new TerrainPoint(0f, 0f);
            var p2 = new TerrainPoint(widthInMeters, widthInMeters);

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
                P1 = p1,
                P2 = p2,
                ImageryResolution = resolution ?? 1d,
                Polygon = TerrainPolygon.FromRectangle(p1, p2)
            };
        }

        public PointF LatLngToPixelsPoint(GeoAPI.Geometries.Coordinate n)
        {
            var coord = new Coordinate(n.Y, n.X, eagerUTM);
            if (coord.UTM.LongZone != StartPointUTM.LongZone)
            {
                coord.Lock_UTM_MGRS_Zone(StartPointUTM.LongZone);
            }
            return new PointF(
                (float)((coord.UTM.Easting - StartPointUTM.Easting) / ImageryResolution),
                (float)((Height - (coord.UTM.Northing - StartPointUTM.Northing)) / ImageryResolution)
            );
        }

        public IEnumerable<PointF> LatLngToPixelsPoints(IEnumerable<GeoAPI.Geometries.Coordinate> nodes)
        {
            return nodes.Select(LatLngToPixelsPoint);
        }

        public TerrainPoint LatLngToTerrainPoint(GeoAPI.Geometries.Coordinate n)
        {
            var coord = new Coordinate(n.Y, n.X, eagerUTM);
            if (coord.UTM.LongZone != StartPointUTM.LongZone)
            {
                coord.Lock_UTM_MGRS_Zone(StartPointUTM.LongZone);
            }
            return new TerrainPoint((float)(coord.UTM.Easting - StartPointUTM.Easting), (float)(coord.UTM.Northing - StartPointUTM.Northing));
        }

        public IEnumerable<TerrainPoint> LatLngToTerrainPoints(IEnumerable<GeoAPI.Geometries.Coordinate> nodes)
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

        public PointF TerrainToPixelsPoint(GeoAPI.Geometries.Coordinate point)
        {
            return new PointF(
                (float)(point.X / ImageryResolution),
                (float)((Height - point.Y) / ImageryResolution)
            );
        }

        public IEnumerable<PointF> TerrainToPixelsPoints(IEnumerable<GeoAPI.Geometries.Coordinate> points)
        {
            return points.Select(TerrainToPixelsPoint);
        }

        public TerrainPoint LatLngToTerrainPoint(OsmSharp.Node node)
        {
            var coord = new Coordinate(node.Latitude.Value, node.Longitude.Value, eagerUTM);

            if (coord.UTM.LongZone != StartPointUTM.LongZone)
            {
                coord.Lock_UTM_MGRS_Zone(StartPointUTM.LongZone);
            }

            return new TerrainPoint(
                    (float)(coord.UTM.Easting - StartPointUTM.Easting),
                    (float)(coord.UTM.Northing - StartPointUTM.Northing)
                );
        }

        public Coordinate TerrainToLatLong(double x, double y)
        {
            return TerrainToLatLong(StartPointUTM, x, y, eagerNONE);
        }

        private static Coordinate TerrainToLatLong(UniversalTransverseMercator startPointUTM, double x, double y, EagerLoad eager)
        {
            var utm = new UniversalTransverseMercator(
                                    startPointUTM.LatZone,
                                    startPointUTM.LongZone,
                                    startPointUTM.Easting + x,
                                    startPointUTM.Northing + y);

            return UniversalTransverseMercator.ConvertUTMtoLatLong(utm, eager ?? GlobalSettings.Default_EagerLoad);
        }

        public string ToCoordinates()
        {
            return string.Join(" ", new[] {
            SouthEast, SouthWest, NorthWest, NorthEast, SouthEast


            }.Select(c => FormattableString.Invariant($"{c.Longitude.ToDouble()},{c.Latitude.ToDouble()},0.0"))

            );


        }
    }
}