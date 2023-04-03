using CoordinateSharp;
using GameRealisticMap.Geometries;
using Coordinate = GeoAPI.Geometries.Coordinate;

namespace GameRealisticMap
{
    public class TerrainAreaUTM : ITerrainArea
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true, Extensions = new EagerLoad_Extensions() { MGRS = false } };
        private static readonly EagerLoad eagerNONE = new EagerLoad(false);

        private readonly UniversalTransverseMercator startPointUTM;

        public TerrainAreaUTM(UniversalTransverseMercator startPointUTM, float gridCellSize, int gridSize)
        {
            this.startPointUTM = startPointUTM;
            GridCellSize = gridCellSize;
            GridSize = gridSize;
            SizeInMeters = gridCellSize * gridSize;
            TerrainBounds = TerrainPolygon.FromRectangle(TerrainPoint.Empty, new TerrainPoint(SizeInMeters, SizeInMeters));
        }

        public static TerrainAreaUTM CreateFromSouthWest(string southWest, float gridCellSize, int gridSize)
        {
            return CreateFromSouthWest(CoordinateSharp.Coordinate.Parse(southWest), gridCellSize, gridSize);
        }

        public static TerrainAreaUTM CreateFromSouthWest(CoordinateSharp.Coordinate southWest, float gridCellSize, int gridSize)
        {
            return new TerrainAreaUTM(southWest.UTM, gridCellSize, gridSize);
        }

        public static TerrainAreaUTM CreateFromCenter(string center, float gridCellSize, int gridSize)
        {
            return CreateFromCenter(CoordinateSharp.Coordinate.Parse(center), gridCellSize, gridSize);
        }

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

        public IEnumerable<Coordinate> GetLatLngBounds()
        {
            return TerrainBounds.Shell.Select(TerrainPointToLatLng);
        }

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
