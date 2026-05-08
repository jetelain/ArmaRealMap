using CoordinateSharp;
using GameRealisticMap.Geometries;
using Coordinate = GeoAPI.Geometries.Coordinate;

namespace GameRealisticMap
{
    /// <summary>
    /// Standard <see cref="ITerrainArea"/> implementation that uses a UTM (Universal Transverse Mercator)
    /// projection for WGS-84 ↔ terrain-space coordinate conversion.
    /// The south-west corner of the terrain area is pinned to a fixed UTM easting/northing;
    /// all <see cref="Geometries.TerrainPoint"/> values are offsets in metres from that origin.
    /// </summary>
    public class TerrainAreaUTM : ITerrainArea
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true, Extensions = new EagerLoad_Extensions() { MGRS = false } };
        private static readonly EagerLoad eagerNONE = new EagerLoad(false);

        private readonly UniversalTransverseMercator startPointUTM;

        /// <summary>
        /// Initialises a terrain area from a known UTM south-west corner.
        /// </summary>
        /// <param name="startPointUTM">UTM coordinate of the south-west corner (origin point of the terrain).</param>
        /// <param name="gridCellSize">Size of each heightmap grid cell in metres.</param>
        /// <param name="gridSize">Number of grid cells along each axis (terrain is square).</param>
        public TerrainAreaUTM(UniversalTransverseMercator startPointUTM, float gridCellSize, int gridSize)
        {
            this.startPointUTM = startPointUTM;
            GridCellSize = gridCellSize;
            GridSize = gridSize;
            SizeInMeters = gridCellSize * gridSize;
            TerrainBounds = TerrainPolygon.FromRectangle(TerrainPoint.Empty, new TerrainPoint(SizeInMeters, SizeInMeters));
        }

        /// <summary>
        /// Creates a <see cref="TerrainAreaUTM"/> from a WGS-84 string describing the south-west corner.
        /// The string is parsed by CoordinateSharp (e.g. <c>"48.666667 N, 6.166667 E"</c>).
        /// </summary>
        public static TerrainAreaUTM CreateFromSouthWest(string southWest, float gridCellSize, int gridSize)
        {
            return CreateFromSouthWest(CoordinateSharp.Coordinate.Parse(southWest), gridCellSize, gridSize);
        }

        /// <summary>
        /// Creates a <see cref="TerrainAreaUTM"/> from a CoordinateSharp coordinate representing the south-west corner.
        /// </summary>
        public static TerrainAreaUTM CreateFromSouthWest(CoordinateSharp.Coordinate southWest, float gridCellSize, int gridSize)
        {
            return new TerrainAreaUTM(southWest.UTM, gridCellSize, gridSize);
        }

        /// <summary>
        /// Creates a <see cref="TerrainAreaUTM"/> from a WGS-84 string describing the map center.
        /// The south-west corner is computed by subtracting half the total terrain size.
        /// </summary>
        public static TerrainAreaUTM CreateFromCenter(string center, float gridCellSize, int gridSize)
        {
            return CreateFromCenter(CoordinateSharp.Coordinate.Parse(center), gridCellSize, gridSize);
        }

        /// <summary>
        /// Creates a <see cref="TerrainAreaUTM"/> from a CoordinateSharp coordinate representing the map center.
        /// </summary>
        public static TerrainAreaUTM CreateFromCenter(CoordinateSharp.Coordinate center, float gridCellSize, int gridSize)
        {
            var halfSize = gridCellSize * gridSize / 2;
            var southWest = new UniversalTransverseMercator(
                center.UTM.LatZone,
                center.UTM.LongZone,
                center.UTM.Easting - halfSize,
                center.UTM.Northing - halfSize);
            return new TerrainAreaUTM(southWest, gridCellSize, gridSize);
        }

        public TerrainPolygon TerrainBounds { get; }

        public float GridCellSize { get; }

        public int GridSize { get; }

        public float SizeInMeters { get; }

        public TerrainPoint LatLngToTerrainPoint(Coordinate latLng)
        {
            var coord = new CoordinateSharp.Coordinate(latLng.Y, latLng.X, eagerUTM);
            if (coord.UTM.LongZone != startPointUTM.LongZone)
            {
                coord.Lock_UTM_MGRS_Zone(startPointUTM.LongZone);
            }
            return new TerrainPoint((float)(coord.UTM.Easting - startPointUTM.Easting), (float)(coord.UTM.Northing - startPointUTM.Northing));
        }

        public Coordinate TerrainPointToLatLng(TerrainPoint point)
        {
            var utm = new UniversalTransverseMercator(
                        startPointUTM.LatZone,
                        startPointUTM.LongZone,
                        startPointUTM.Easting + point.X,
                        startPointUTM.Northing + point.Y);

            var coord = UniversalTransverseMercator.ConvertUTMtoLatLong(utm, eagerNONE);

            return new Coordinate(coord.Longitude.ToDouble(), coord.Latitude.ToDouble());
        }
    }
}
